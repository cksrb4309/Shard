mergeInto(LibraryManager.library, {
  Shard_BlockWebGLMouseContext: function () {
    if (typeof window === "undefined" || window.__shardWebGLMouseContextBlocked) {
      return;
    }

    window.__shardWebGLMouseContextBlocked = true;

    var preventContextMenu = function (event) {
      event.preventDefault();
    };

    var preventRightAuxClick = function (event) {
      if (event.button === 2) {
        event.preventDefault();
      }
    };

    document.addEventListener("contextmenu", preventContextMenu, { capture: true });
    document.addEventListener("auxclick", preventRightAuxClick, { capture: true });
  },

  Shard_SetWebGLCanvasCursorHidden: function (hidden) {
    if (typeof window === "undefined") {
      return;
    }

    window.__shardWebGLCursorHidden = hidden !== 0;

    var getCanvas = function () {
      if (typeof Module !== "undefined" && Module && Module.canvas) {
        return Module.canvas;
      }

      return document.querySelector("canvas");
    };

    var applyCursor = function () {
      var canvas = getCanvas();
      if (!canvas) {
        return;
      }

      if (window.__shardWebGLCursorHidden) {
        canvas.style.setProperty("cursor", "none", "important");
      } else {
        canvas.style.removeProperty("cursor");
      }
    };

    applyCursor();

    if (window.__shardWebGLCursorHooked) {
      return;
    }

    window.__shardWebGLCursorHooked = true;

    var reapply = function () {
      applyCursor();
    };

    document.addEventListener("visibilitychange", reapply);
    window.addEventListener("focus", reapply);
    window.addEventListener("resize", reapply);
    document.addEventListener("mousemove", reapply, { passive: true });
  }
});
