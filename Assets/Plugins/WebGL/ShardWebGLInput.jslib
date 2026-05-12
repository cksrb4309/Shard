mergeInto(LibraryManager.library, {
  Shard_BlockWebGLMouseContext: function () {
    if (typeof window === "undefined" || window.__shardWebGLMouseContextBlocked) {
      return;
    }

    window.__shardWebGLMouseContextBlocked = true;

    window.__shardWebGLMouseButtonState = window.__shardWebGLMouseButtonState || {
      downQueue: [0, 0, 0, 0, 0],
      pressed: [false, false, false, false, false]
    };

    var getMouseButton = function (event) {
      if (!event || typeof event.button !== "number" || event.button < 0 || event.button > 4) {
        return -1;
      }

      return event.button;
    };

    var preventContextMenu = function (event) {
      event.preventDefault();
    };

    var preventRightAuxClick = function (event) {
      if (event.button === 2) {
        event.preventDefault();
      }
    };

    var handleMouseDown = function (event) {
      var button = getMouseButton(event);
      if (button < 0) {
        return;
      }

      window.__shardWebGLMouseButtonState.pressed[button] = true;

      if (button === 2) {
        window.__shardWebGLMouseButtonState.downQueue[button] += 1;
        event.preventDefault();
      }
    };

    var handleMouseUp = function (event) {
      var button = getMouseButton(event);
      if (button < 0) {
        return;
      }

      window.__shardWebGLMouseButtonState.pressed[button] = false;

      if (button === 2) {
        event.preventDefault();
      }
    };

    var clearMouseButtons = function () {
      var state = window.__shardWebGLMouseButtonState;
      for (var i = 0; i < state.pressed.length; i++) {
        state.pressed[i] = false;
        state.downQueue[i] = 0;
      }
    };

    document.addEventListener("contextmenu", preventContextMenu, { capture: true });
    document.addEventListener("auxclick", preventRightAuxClick, { capture: true });
    document.addEventListener("mousedown", handleMouseDown, { capture: true });
    document.addEventListener("mouseup", handleMouseUp, { capture: true });
    window.addEventListener("blur", clearMouseButtons);
  },

  Shard_ConsumeWebGLMouseButtonDown: function (button) {
    if (typeof window === "undefined" || !window.__shardWebGLMouseButtonState) {
      return 0;
    }

    var state = window.__shardWebGLMouseButtonState;
    if (button < 0 || button >= state.downQueue.length || state.downQueue[button] <= 0) {
      return 0;
    }

    state.downQueue[button] -= 1;
    return 1;
  },

  Shard_ClearWebGLMouseButtonDown: function (button) {
    if (typeof window === "undefined" || !window.__shardWebGLMouseButtonState) {
      return;
    }

    var state = window.__shardWebGLMouseButtonState;
    if (button >= 0 && button < state.downQueue.length) {
      state.downQueue[button] = 0;
    }
  },

  Shard_IsWebGLMouseButtonPressed: function (button) {
    if (typeof window === "undefined" || !window.__shardWebGLMouseButtonState) {
      return 0;
    }

    var state = window.__shardWebGLMouseButtonState;
    if (button < 0 || button >= state.pressed.length) {
      return 0;
    }

    return state.pressed[button] ? 1 : 0;
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
