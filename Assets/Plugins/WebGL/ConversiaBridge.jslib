mergeInto(LibraryManager.library, {
  ConversiaBridge_PostMessage: function (messagePtr) {
    if (!messagePtr) {
      return;
    }
    var raw = UTF8ToString(messagePtr);
    if (!raw) {
      return;
    }
    try {
      var payload = JSON.parse(raw);
      if (typeof payload !== 'object' || payload === null) {
        return;
      }
      payload.source = 'game';
      if (typeof window !== 'undefined' && window.parent) {
        window.parent.postMessage(payload, '*');
      }
    } catch (err) {
      console.error('[ConversiaBridge] Failed to post message from Unity', err, raw);
    }
  }
});
