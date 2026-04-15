import argparse
import datetime
import importlib
import inspect
import json
import shutil
import sys
import tempfile
from pathlib import Path


def _filter_kwargs(func, kwargs):
    try:
        sig = inspect.signature(func)
    except (TypeError, ValueError):
        return kwargs
    params = sig.parameters
    if any(p.kind == inspect.Parameter.VAR_KEYWORD for p in params.values()):
        return kwargs
    return {k: v for k, v in kwargs.items() if k in params}


def _call_stage(module_name, func_candidates, **kwargs):
    mod = importlib.import_module(module_name)
    for func_name in func_candidates:
        func = getattr(mod, func_name, None)
        if func is None:
            continue
        filtered = _filter_kwargs(func, kwargs)
        try:
            return func(**filtered)
        except TypeError:
            try:
                sig = inspect.signature(func)
                ordered = []
                for name in sig.parameters:
                    if name in filtered:
                        ordered.append(filtered[name])
                return func(*ordered)
            except Exception:
                raise
    raise AttributeError(f"No callable found in {module_name} (tried {', '.join(func_candidates)})")


def _extract_files(detect_result):
    if detect_result is None:
        return []
    if isinstance(detect_result, (list, tuple, set)):
        return list(detect_result)
    if isinstance(detect_result, dict):
        for key in ("files", "code_files", "paths", "file_paths", "items"):
            val = detect_result.get(key)
            if isinstance(val, (list, tuple, set)):
                return list(val)
    return []


def _ensure_manifest(out_dir, root, files):
    manifest_path = out_dir / "manifest.json"
    if manifest_path.exists():
        return
    manifest = {
        "generated_at": datetime.datetime.utcnow().isoformat() + "Z",
        "source_root": str(root),
        "files": [p.name for p in files],
    }
    manifest_path.write_text(json.dumps(manifest, indent=2), encoding="utf-8")


def main(argv):
    parser = argparse.ArgumentParser(description="Explicit graphify refresh workflow.")
    parser.add_argument("project_root", nargs="?", default=".", help="Project root (default: .)")
    parser.add_argument(
        "--obsidian",
        action="store_true",
        help="Also generate an Obsidian vault under graphify-out/obsidian",
    )
    args = parser.parse_args(argv)

    root = Path(args.project_root).resolve()
    if not root.exists():
        print(f"[graphify-refresh] Project root not found: {root}")
        return 1

    try:
        importlib.import_module("graphify")
    except Exception:
        print("[graphify-refresh] graphify is not installed. Install graphify first.")
        return 1

    with tempfile.TemporaryDirectory(prefix=".graphify_tmp_", dir=root) as tmp_dir:
        tmp_root = Path(tmp_dir)
        tmp_work = tmp_root / "work"
        tmp_out = tmp_root / "graphify-out"
        tmp_cache = tmp_root / "cache"
        tmp_work.mkdir(parents=True, exist_ok=True)
        tmp_out.mkdir(parents=True, exist_ok=True)
        tmp_cache.mkdir(parents=True, exist_ok=True)

        detect_result = _call_stage(
            "graphify.detect",
            ["detect", "run", "main"],
            project_root=root,
            root=root,
            output_dir=tmp_work,
            cache_dir=tmp_cache,
        )
        code_files = _extract_files(detect_result)
        if not code_files:
            print("[graphify-refresh] No code files found. Nothing to do.")
            return 0

        extracted = _call_stage(
            "graphify.extract",
            ["extract", "run", "main"],
            project_root=root,
            input_files=code_files,
            output_dir=tmp_work,
            cache_dir=tmp_cache,
        )
        graph = _call_stage(
            "graphify.build",
            ["build", "run", "main"],
            project_root=root,
            extracted=extracted,
            output_dir=tmp_work,
        )
        clustered = _call_stage(
            "graphify.cluster",
            ["cluster", "run", "main"],
            project_root=root,
            graph=graph,
            output_dir=tmp_work,
        )
        analysis = _call_stage(
            "graphify.analyze",
            ["analyze", "run", "main"],
            project_root=root,
            graph=clustered,
            output_dir=tmp_work,
        )
        report = _call_stage(
            "graphify.report",
            ["report", "run", "main"],
            project_root=root,
            graph=clustered,
            analysis=analysis,
            output_dir=tmp_out,
        )
        _call_stage(
            "graphify.export",
            ["export", "run", "main"],
            project_root=root,
            graph=clustered,
            analysis=analysis,
            report=report,
            output_dir=tmp_out,
            obsidian=args.obsidian,
            obsidian_dir=(tmp_out / "obsidian"),
        )

        required = [
            tmp_out / "GRAPH_REPORT.md",
            tmp_out / "graph.json",
            tmp_out / "graph.html",
            tmp_out / "manifest.json",
        ]
        if args.obsidian:
            required.append(tmp_out / "obsidian")
        _ensure_manifest(tmp_out, root, required)

        missing = [p.name for p in required if not p.exists()]
        if missing:
            print(f"[graphify-refresh] Missing outputs: {', '.join(missing)}")
            return 1

        final_out = root / "graphify-out"
        if final_out.exists():
            shutil.rmtree(final_out)
        shutil.copytree(tmp_out, final_out)

    print(f"[graphify-refresh] Updated {final_out}")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
