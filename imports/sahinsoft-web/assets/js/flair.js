/* ŞahinSoft — Flair Layer (progressive enhancement, hiçbir içeriği gizlemez) */
(function () {
  "use strict";

  function ready(fn) {
    if (document.readyState !== "loading") fn();
    else document.addEventListener("DOMContentLoaded", fn);
  }

  ready(function () {
    try { injectBlobs(); } catch (e) {}
    try { attachRipple(); } catch (e) {}
    try { animateStats(); } catch (e) {}
  });

  // 1) Ambient gradient blobs — decorative only, pointer-events disabled
  function injectBlobs() {
    if (document.getElementById("flair-blobs")) return;
    var wrap = document.createElement("div");
    wrap.id = "flair-blobs";
    wrap.setAttribute("aria-hidden", "true");
    for (var i = 0; i < 4; i++) {
      var b = document.createElement("span");
      b.className = "flair-blob";
      wrap.appendChild(b);
    }
    document.body.insertBefore(wrap, document.body.firstChild);
  }

  // 2) Soft ripple effect on buttons
  function attachRipple() {
    var buttons = document.querySelectorAll(".btn, a.btn, .btn-gold");
    buttons.forEach(function (btn) {
      btn.addEventListener("click", function (e) {
        var rect = btn.getBoundingClientRect();
        var span = document.createElement("span");
        var size = Math.max(rect.width, rect.height);
        span.className = "flair-ripple";
        span.style.width = span.style.height = size + "px";
        span.style.left = (e.clientX - rect.left - size / 2) + "px";
        span.style.top = (e.clientY - rect.top - size / 2) + "px";
        btn.appendChild(span);
        setTimeout(function () { span.remove(); }, 650);
      });
    });
  }

  // 3) Count-up animation for the stats section (index.html)
  function animateStats() {
    var nodes = document.querySelectorAll('#istatistikler div[style*="font-size: 42px"]');
    if (!nodes.length || !("IntersectionObserver" in window)) return;

    var done = false;
    var obs = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting && !done) {
          done = true;
          nodes.forEach(runCounter);
          obs.disconnect();
        }
      });
    }, { threshold: 0.4 });

    nodes.forEach(function (n) { obs.observe(n); });

    function runCounter(node) {
      var raw = node.textContent.trim();
      var match = raw.match(/[\d.,]+/);
      if (!match) return;
      var numStr = match[0].replace(/\./g, "").replace(",", ".");
      var target = parseFloat(numStr);
      if (isNaN(target)) return;
      var prefix = raw.slice(0, match.index);
      var suffix = raw.slice(match.index + match[0].length);
      var isDecimal = numStr.indexOf(".") !== -1;
      var duration = 1200;
      var start = null;

      function step(ts) {
        if (start === null) start = ts;
        var progress = Math.min((ts - start) / duration, 1);
        var eased = 1 - Math.pow(1 - progress, 3);
        var current = target * eased;
        node.textContent = prefix + (isDecimal ? current.toFixed(1) : Math.round(current)) + suffix;
        if (progress < 1) requestAnimationFrame(step);
        else node.textContent = raw;
      }
      requestAnimationFrame(step);
    }
  }
})();
