// Shared behavior for the Abstract Pixel Library docs. Offline: no CDN, no build step.
// Rules: DOCS-STANDARD.md at the library root. Every page loads this after assets/docs.css.
(function () {
  "use strict";

  var MODULE_HOME = { href: "index.html", label: "Save System" };

  var NAV = [
    {
      group: "Guide",
      items: [
        ["Pages/get-started.html", "Get started"],
        ["Pages/save-data.html", "Save, restore and react to data"],
        ["Pages/save-files.html", "Choose where data goes"],
        ["Pages/save-objects.html", "Save plain classes, ScriptableObjects and spawned copies"],
        ["Pages/custom-data.html", "Saving complex data with ISavable"],
        ["Pages/remove-for-good.html", "Forgetting saved data for good"],
        ["Pages/save-slots.html", "Game profiles and save slots"],
        ["Pages/events.html", "React to loads and saves"],
        ["Pages/whole-files.html", "Read or write a whole file"],
        ["Pages/debug.html", "Debug and inspect"]
      ]
    },
    {
      group: "Reference",
      items: [
        ["Pages/code-map.html", "Code map"],
        ["Pages/problems.html", "Find a problem"]
      ]
    },
    {
      group: "Types",
      sub: true,
      // Grouped by responsibility, matching the public-type rows in Pages/code-map.html, instead of one flat
      // alphabetical list: a reader can open just the cluster of types they're looking for.
      groups: [
        {
          name: "Configuration",
          items: [
            ["api/SaveSystemConfigSO.html", "SaveSystemConfigSO"],
            ["api/SaveLocation.html", "SaveLocation"],
            ["api/FileExtension.html", "FileExtension"],
            ["api/SaveStartupPolicy.html", "SaveStartupPolicy"]
          ]
        },
        {
          name: "Game profiles",
          items: [
            ["api/GameProfile.html", "GameProfile"],
            ["api/GameProfileList.html", "GameProfileList"]
          ]
        },
        {
          name: "Save IDs",
          items: [
            ["api/SavableBridge.html", "SavableBridge"],
            ["api/SavedScriptExtensions.html", "SavedScriptExtensions"]
          ]
        },
        {
          name: "Saving and loading",
          items: [
            ["api/SaveActions.html", "SaveActions"],
            ["api/SaveEventBus.html", "SaveEventBus"],
            ["api/SaveStorage.html", "SaveStorage"]
          ]
        },
        {
          name: "Save files",
          items: [
            ["api/SaveFiles.html", "SaveFiles"],
            ["api/ISaveFile.html", "ISaveFile"],
            ["api/SaveFileDefaultsAttribute.html", "SaveFileDefaultsAttribute"],
            ["api/SaveFileReference.html", "SaveFileReference"]
          ]
        },
        {
          name: "Attributes & interfaces",
          items: [
            ["api/SaveAttribute.html", "SaveAttribute"],
            ["api/SaveIdAttribute.html", "SaveIdAttribute"],
            ["api/ISavable.html", "ISavable<TData>"],
            ["api/ISaveIn.html", "ISaveIn<TFile>"],
            ["api/IOnRestored.html", "IOnRestored"]
          ]
        },
        {
          name: "No-code components",
          items: [
            ["api/SaveTransform.html", "SaveTransform"],
            ["api/SaveActiveState.html", "SaveActiveState"],
            ["api/SaveDestroyed.html", "SaveDestroyed"],
            ["api/MonoSaveActions.html", "MonoSaveActions"]
          ]
        }
      ]
    }
  ];

  // A section has either a flat "items" list, or a "groups" list of { name, items } for a tree with
  // one more level. Either way, this flattens it to a plain items list.
  function sectionItems(section) {
    if (section.items) {
      return section.items;
    }
    return section.groups.reduce(function (all, group) {
      return all.concat(group.items);
    }, []);
  }

  var FLAT_PAGES = [MODULE_HOME].concat(
    NAV.reduce(function (all, section) {
      return all.concat(sectionItems(section).map(function (item) {
        return { href: item[0], label: item[1] };
      }));
    }, [])
  );

  var GROUP_BY_HREF = {};
  NAV.forEach(function (section) {
    sectionItems(section).forEach(function (item) {
      GROUP_BY_HREF[item[0]] = section.group;
    });
  });

  function root() {
    return document.body.getAttribute("data-root") || "";
  }

  function currentPage() {
    return document.body.getAttribute("data-page") || "";
  }

  function el(tag, attrs, html) {
    var node = document.createElement(tag);
    for (var key in attrs) {
      if (Object.prototype.hasOwnProperty.call(attrs, key)) {
        node.setAttribute(key, attrs[key]);
      }
    }
    if (html != null) {
      node.innerHTML = html;
    }
    return node;
  }

  function escapeHtml(text) {
    return text.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
  }

  // A heading's own text, ignoring a trailing ladder pill (<span class="lv ...">) or anchor link (<a class="anchor">).
  function headingLabel(h) {
    return h && h.firstChild && h.firstChild.nodeType === Node.TEXT_NODE ? h.firstChild.textContent.trim() : h ? h.textContent.trim() : "";
  }

  function buildTree() {
    var tree = document.getElementById("tree");
    if (!tree) {
      return;
    }
    var page = currentPage();
    var module = el("a", { class: "module", href: root() + MODULE_HOME.href }, escapeHtml(MODULE_HOME.label));
    if (page === MODULE_HOME.href) {
      module.classList.add("current");
    }
    tree.appendChild(module);

    function renderItems(list, items, sub) {
      items.forEach(function (item) {
        var classes = "item" + (sub ? " sub" : "");
        if (item[0] === page) {
          classes += " current";
        }
        list.appendChild(el("li", {}, '<a class="' + classes + '" href="' + root() + item[0] + '">' + escapeHtml(item[1]) + "</a>"));
      });
    }

    function addToggle(button, list, startOpen) {
      list.hidden = !startOpen;
      button.setAttribute("aria-expanded", startOpen ? "true" : "false");
      button.addEventListener("click", function () {
        var expanded = button.getAttribute("aria-expanded") === "true";
        button.setAttribute("aria-expanded", expanded ? "false" : "true");
        list.hidden = expanded;
      });
    }

    NAV.forEach(function (section) {
      var hasCurrent = sectionItems(section).some(function (item) { return item[0] === page; });
      var button = el("button", { type: "button", class: "group" }, escapeHtml(section.group) + ' <span class="caret">▾</span>');
      var list = el("ol", { class: "group-items" });
      if (section.groups) {
        section.groups.forEach(function (group) {
          var groupHasCurrent = group.items.some(function (item) { return item[0] === page; });
          var subButton = el("button", { type: "button", class: "subgroup" }, escapeHtml(group.name) + ' <span class="caret">▾</span>');
          var subList = el("ol", { class: "group-items" });
          renderItems(subList, group.items, true);
          addToggle(subButton, subList, groupHasCurrent);
          list.appendChild(el("li", { class: "subgroup-item" }));
          list.lastChild.appendChild(subButton);
          list.lastChild.appendChild(subList);
        });
      } else {
        renderItems(list, section.items, section.sub);
      }
      addToggle(button, list, hasCurrent);
      tree.appendChild(button);
      tree.appendChild(list);
    });
  }

  function buildCrumbs() {
    var crumbs = document.getElementById("crumbs");
    if (!crumbs) {
      return;
    }
    var page = currentPage();
    if (!page || page === MODULE_HOME.href) {
      crumbs.innerHTML = escapeHtml(MODULE_HOME.label);
      return;
    }
    var group = GROUP_BY_HREF[page];
    var title = headingLabel(document.querySelector("main h1")) || page;
    var parts = ['<a href="' + root() + MODULE_HOME.href + '">' + escapeHtml(MODULE_HOME.label) + "</a>"];
    if (group) {
      parts.push(escapeHtml(group));
    }
    parts.push(escapeHtml(title));
    crumbs.innerHTML = parts.join(" &rsaquo; ");
  }

  function buildPager() {
    var pager = document.getElementById("pager");
    if (!pager) {
      return;
    }
    var page = currentPage();
    var index = -1;
    for (var i = 0; i < FLAT_PAGES.length; i++) {
      if (FLAT_PAGES[i].href === page) {
        index = i;
        break;
      }
    }
    if (index < 0) {
      return;
    }
    var prev = FLAT_PAGES[index - 1];
    var next = FLAT_PAGES[index + 1];
    pager.innerHTML =
      (prev ? '<a href="' + root() + prev.href + '">&larr; ' + escapeHtml(prev.label) + "</a>" : "<span></span>") +
      (next ? '<a href="' + root() + next.href + '">' + escapeHtml(next.label) + " &rarr;</a>" : "<span></span>");
  }

  // A page section is <section id="..."><h2>Heading<a class="anchor">#</a></h2>...</section>: the id names
  // the section, not the heading, so both the toc and the scrollspy key off section[id] rather than h2[id].
  function sections() {
    var main = document.querySelector("main");
    if (!main) {
      return [];
    }
    return Array.prototype.slice.call(main.querySelectorAll("section[id]")).filter(function (s) {
      return s.querySelector("h2, h3");
    });
  }

  function buildToc() {
    var toc = document.getElementById("toc");
    var secs = sections();
    if (!toc || secs.length === 0) {
      return;
    }
    toc.appendChild(el("div", { class: "label" }, "On this page"));
    secs.forEach(function (s) {
      var text = headingLabel(s.querySelector("h2, h3"));
      toc.appendChild(el("a", { href: "#" + s.id, "data-toc-for": s.id }, escapeHtml(text)));
    });

    var links = Array.prototype.slice.call(toc.querySelectorAll("a[data-toc-for]"));
    if (!("IntersectionObserver" in window) || links.length === 0) {
      return;
    }
    var active = null;
    function setActive(id) {
      if (active) {
        active.classList.remove("active");
      }
      active = null;
      links.forEach(function (link) {
        if (link.getAttribute("data-toc-for") === id) {
          link.classList.add("active");
          active = link;
        }
      });
    }
    var observer = new IntersectionObserver(
      function (entries) {
        var topMost = null;
        entries.forEach(function (entry) {
          if (entry.isIntersecting) {
            if (!topMost || entry.boundingClientRect.top < topMost.boundingClientRect.top) {
              topMost = entry;
            }
          }
        });
        if (topMost) {
          var id = topMost.target.id;
          setActive(id);
          if (history.replaceState) {
            history.replaceState(null, "", "#" + id);
          }
        }
      },
      { rootMargin: "-10% 0px -70% 0px" }
    );
    secs.forEach(function (s) {
      observer.observe(s);
    });
  }

  function addHeadingAnchors() {
    sections().forEach(function (s) {
      var h = s.querySelector("h2, h3");
      if (h.querySelector(".anchor")) {
        return;
      }
      h.appendChild(el("a", { class: "anchor", href: "#" + s.id, "aria-label": "Link to this section" }, "#"));
    });
    Array.prototype.slice.call(document.querySelectorAll("main h2[id], main h3[id]")).forEach(function (h) {
      if (h.querySelector(".anchor")) {
        return;
      }
      h.appendChild(el("a", { class: "anchor", href: "#" + h.id, "aria-label": "Link to this section" }, "#"));
    });
  }

  // Modifiers (who can see/override a member), built-in value/reference types (treated the same as a
  // custom type, so a return type reads consistently whether it's "void" or "GameProfile"), and
  // everything else. Keeping these three lists apart is what lets a member's access level stand out
  // from its type, and its type read the same everywhere it appears (return type, parameter, field).
  var MODIFIER_WORDS = [
    "abstract", "async", "const", "extern", "internal", "override", "partial", "private", "protected",
    "public", "readonly", "sealed", "static", "unsafe", "virtual", "volatile"
  ];
  var TYPE_WORDS = [
    "bool", "byte", "char", "decimal", "double", "float", "int", "long", "object", "sbyte", "short",
    "string", "uint", "ulong", "ushort", "var", "void"
  ];
  var KEYWORDS = [
    "as", "await", "base", "break", "case", "catch", "checked", "class", "continue", "default", "delegate",
    "do", "else", "enum", "event", "explicit", "false", "finally", "fixed", "for", "foreach", "get", "goto",
    "if", "implicit", "in", "interface", "is", "lock", "namespace", "new", "null", "operator", "out",
    "params", "ref", "return", "set", "sizeof", "stackalloc", "struct", "switch", "this", "throw", "true",
    "try", "typeof", "unchecked", "using", "value", "where", "while", "yield"
  ];

  // A "Type name" pair followed by , ) or = is a parameter (or a declared out/catch variable): matching
  // both words as one token lets the type keep its normal type colour while the name gets its own.
  var PARAM_PAIR = "\\b([A-Za-z_]\\w*(?:<[^<>()]*>)?(?:\\[\\])?\\??)[ \\t]+([A-Za-z_]\\w*)\\b(?=\\s*[,)=])";

  var CS_TOKEN = new RegExp(
    "(/\\*[\\s\\S]*?\\*/)" +
      "|(//[^\\n]*)" +
      '|(@"(?:[^"]|"")*")' +
      '|(\\$@"(?:[^"]|"")*")' +
      '|(\\$"(?:[^"\\\\]|\\\\.)*")' +
      '|("(?:[^"\\\\]|\\\\.)*")' +
      "|('(?:[^'\\\\]|\\\\.)*')" +
      "|(\\[[A-Za-z_]\\w*(?:\\.\\w+)*(?:\\s*\\([^)]*\\))?\\])" +
      "|" + PARAM_PAIR +
      "|\\b(" + MODIFIER_WORDS.join("|") + ")\\b" +
      "|\\b(" + TYPE_WORDS.join("|") + ")\\b" +
      "|\\b(" + KEYWORDS.join("|") + ")\\b" +
      "|\\b([A-Za-z_]\\w*)(?=\\()" +
      "|\\b([A-Z]\\w*)\\b" +
      "|\\b(\\d+\\.?\\d*[fFdDmMuUlL]?)\\b",
    "g"
  );

  function highlightCSharp(code) {
    var out = "";
    var last = 0;
    var match;
    // True right after ( or , : the position a parameter name (not a field or local) can start.
    var argStart = false;
    function trackArgState(raw) {
      for (var i = 0; i < raw.length; i++) {
        var c = raw[i];
        if (c === "(" || c === ",") { argStart = true; }
        else if (c === ")") { argStart = false; }
        else if (!/\s/.test(c)) { argStart = false; }
      }
    }
    CS_TOKEN.lastIndex = 0;
    while ((match = CS_TOKEN.exec(code))) {
      var gap = code.slice(last, match.index);
      out += escapeHtml(gap);
      trackArgState(gap);

      if (match[9] !== undefined) {
        var typeText = match[9];
        var nameText = match[10];
        var between = match[0].slice(typeText.length, match[0].length - nameText.length);
        var typeClass = TYPE_WORDS.indexOf(typeText) !== -1 || /^[A-Z]/.test(typeText) ? "hl-type" : null;
        out += (typeClass ? '<span class="' + typeClass + '">' + escapeHtml(typeText) + "</span>" : escapeHtml(typeText)) +
          escapeHtml(between) +
          (argStart ? '<span class="hl-param">' + escapeHtml(nameText) + "</span>" : escapeHtml(nameText));
      } else {
        var cls =
          match[1] || match[2] ? "hl-com" :
          match[3] || match[4] || match[5] || match[6] || match[7] ? "hl-str" :
          match[8] ? "hl-attr" :
          match[11] ? "hl-mod" :
          match[12] ? "hl-type" :
          match[13] ? "hl-key" :
          match[14] ? "hl-call" :
          match[15] ? "hl-type" :
          match[16] ? "hl-num" : null;
        var text = escapeHtml(match[0]);
        out += cls ? '<span class="' + cls + '">' + text + "</span>" : text;
      }
      trackArgState(match[0]);
      last = CS_TOKEN.lastIndex;
    }
    out += escapeHtml(code.slice(last));
    return out;
  }

  function highlightCode() {
    // pre[data-source] is a sample README's "lines that matter" block: the docs test reads its raw
    // <code> text, so it can't carry a class="cs" in the file; highlight it here instead. code.cs also
    // matches a Members signature's bare <code class="cs"> (not wrapped in <pre>).
    Array.prototype.slice.call(document.querySelectorAll("code.cs, pre[data-source] code")).forEach(function (block) {
      block.innerHTML = highlightCSharp(block.textContent);
    });
  }

  function addCopyButtons() {
    Array.prototype.slice.call(document.querySelectorAll("figure.code")).forEach(function (figure) {
      var caption = figure.querySelector("figcaption");
      var code = figure.querySelector("pre code");
      if (!caption || !code || figure.querySelector(".copy")) {
        return;
      }
      var button = el("button", { class: "copy", type: "button" }, "Copy");
      button.addEventListener("click", function () {
        var text = code.textContent;
        var done = function () {
          button.textContent = "Copied";
          setTimeout(function () {
            button.textContent = "Copy";
          }, 1500);
        };
        if (navigator.clipboard && navigator.clipboard.writeText) {
          navigator.clipboard.writeText(text).then(done, done);
        } else {
          var area = document.createElement("textarea");
          area.value = text;
          document.body.appendChild(area);
          area.select();
          try {
            document.execCommand("copy");
          } catch (e) {
            // Clipboard not available; the button just won't confirm.
          }
          document.body.removeChild(area);
          done();
        }
      });
      figure.appendChild(button);
    });
  }

  function setupGlossaryTooltips() {
    var glossary = window.DOCS_GLOSSARY || {};
    var terms = Array.prototype.slice.call(document.querySelectorAll("a.term"));
    if (terms.length === 0) {
      return;
    }
    var tip = null;
    function hide() {
      if (tip) {
        tip.remove();
        tip = null;
      }
    }
    terms.forEach(function (link) {
      link.addEventListener("mouseenter", function () {
        var hashIndex = link.getAttribute("href").indexOf("#");
        var id = hashIndex < 0 ? "" : link.getAttribute("href").slice(hashIndex + 1);
        var entry = glossary[id];
        if (!entry) {
          return;
        }
        hide();
        tip = el("div", { class: "tip", role: "tooltip" }, "<strong>" + escapeHtml(entry.term) + "</strong>" + escapeHtml(entry.definition));
        document.body.appendChild(tip);
        var rect = link.getBoundingClientRect();
        tip.style.left = Math.max(8, rect.left + window.scrollX) + "px";
        tip.style.top = rect.bottom + window.scrollY + 6 + "px";
      });
      link.addEventListener("mouseleave", hide);
    });
  }

  var SAMPLE_PATH = /\/Samples\//i;
  // Every numbered sample page lives at Samples/<NN Name>/README.html, two levels below Documentation/,
  // so this is its path back to this page regardless of where in Documentation/ this page itself lives.
  var SAMPLE_TO_DOCS = "../../Documentation/";

  function tagSampleLinks() {
    var page = currentPage();
    if (!page) {
      return;
    }
    Array.prototype.slice.call(document.querySelectorAll("main a[href]")).forEach(function (link) {
      var href = link.getAttribute("href");
      if (!SAMPLE_PATH.test(link.href) || href.indexOf("?") >= 0) {
        return;
      }
      var section = link.closest("section");
      var heading = section ? section.querySelector("h2") : null;
      var label = heading ? headingLabel(heading) : document.title;
      var from = SAMPLE_TO_DOCS + page;
      var query = "?from=" + encodeURIComponent(from) + "&label=" + encodeURIComponent(label);
      var hashIndex = href.indexOf("#");
      link.setAttribute("href", hashIndex >= 0 ? href.slice(0, hashIndex) + query + href.slice(hashIndex) : href + query);
    });
  }

  // The scene button: pings a scene in Unity's Project window if the Editor is running (via a tiny
  // local listener the save system starts on this port), or falls back to copying its path and, if
  // the optional one-time "apscene://" protocol is set up, opening File Explorer there.
  var SCENE_PING_PORT = 58357;
  var SCENE_PING_TIMEOUT_MS = 400;

  function sceneButtonStatus(button, text) {
    var status = button.nextElementSibling;
    if (!status || !status.classList.contains("scene-btn-status")) {
      status = el("span", { class: "scene-btn-status" }, "");
      button.parentNode.insertBefore(status, button.nextSibling);
    }
    status.textContent = text;
  }

  // Tries the optional "apscene://" protocol in a throwaway iframe, so a browser with nothing
  // registered for it can't navigate this page away or show an error in its place.
  function tryOpenProtocol(url) {
    try {
      var frame = document.createElement("iframe");
      frame.style.display = "none";
      frame.src = url;
      document.body.appendChild(frame);
      setTimeout(function () {
        frame.remove();
      }, 1000);
    } catch (e) {
      // Ignored: this is a best-effort extra, not the main fallback (copying the path).
    }
  }

  function setupSceneButtons() {
    Array.prototype.slice.call(document.querySelectorAll("button.scene-btn[data-scene]")).forEach(function (button) {
      button.addEventListener("click", function (event) {
        // A button inside a <summary> (the collapsed Try it box) would otherwise also toggle it open.
        event.stopPropagation();
        var path = button.getAttribute("data-scene");
        sceneButtonStatus(button, "Looking for Unity…");
        if (navigator.clipboard && navigator.clipboard.writeText) {
          navigator.clipboard.writeText(path).catch(function () {});
        }
        var controller = window.AbortController ? new AbortController() : null;
        var timer = controller ? setTimeout(function () { controller.abort(); }, SCENE_PING_TIMEOUT_MS) : null;
        fetch("http://127.0.0.1:" + SCENE_PING_PORT + "/ping?path=" + encodeURIComponent(path), {
          mode: "cors",
          signal: controller ? controller.signal : undefined
        }).then(
          function () {
            clearTimeout(timer);
            sceneButtonStatus(button, "Pinged in Unity's Project window.");
          },
          function () {
            clearTimeout(timer);
            tryOpenProtocol("apscene://" + encodeURIComponent(path));
            sceneButtonStatus(button, "Unity isn't running here. Copied the path to the clipboard — paste it into File Explorer, or open Unity and click again.");
          }
        );
      });
    });
  }

  // The floating "where I came from" trail: a small bar of the last few pages visited, so you can
  // jump back to any of them (not just the one immediately before) without the browser's own Back
  // button. Session-only (sessionStorage), so it never reaches another tab or another visit.
  var TRAIL_KEY = "apdocs-trail-v1";
  var TRAIL_MAX = 6;

  function trailLabel() {
    var h1 = document.querySelector("main h1, h1");
    var label = h1 ? headingLabel(h1) : "";
    return label || document.title.replace(/ - Save System Samples$/, "");
  }

  function loadTrail() {
    try {
      var raw = sessionStorage.getItem(TRAIL_KEY);
      return raw ? JSON.parse(raw) : [];
    } catch (e) {
      return [];
    }
  }

  function saveTrail(trail) {
    try {
      sessionStorage.setItem(TRAIL_KEY, JSON.stringify(trail));
    } catch (e) {
      // sessionStorage unavailable (some browsers restrict it for file:// pages); the trail just
      // won't persist between pages there, but the page itself still works fine without it.
    }
  }

  // A single "where I came from" pointer, not a growing trail: bouncing between two pages back and
  // forth must never pile up repeats, so visiting a page drops any earlier occurrence of it before
  // appending it again. What's shown is just the page right before this one.
  function buildHistoryTrail() {
    var url = window.location.href.split(/[?#]/)[0];
    var label = trailLabel();
    var trail = loadTrail().filter(function (entry) { return entry.url !== url; });
    trail.push({ url: url, label: label });
    if (trail.length > TRAIL_MAX) {
      trail = trail.slice(trail.length - TRAIL_MAX);
    }
    saveTrail(trail);
    if (trail.length < 2) {
      return;
    }
    var prev = trail[trail.length - 2];
    var bar = el("a", { class: "history-trail", href: prev.url, "aria-label": "Back to " + prev.label });
    bar.appendChild(el("span", { class: "history-trail-arrow", "aria-hidden": "true" }, "&larr;"));
    bar.appendChild(el("span", { class: "history-trail-label" }, escapeHtml(prev.label)));
    document.body.appendChild(bar);
  }

  // A Types page's "Open in Visual Studio" button: hands the file to whatever Windows has
  // registered for the "apfile://" protocol (set up once via assets/tools/register-scene-button.ps1),
  // which opens it in your default .cs editor. Also copies the path either way, since there's no
  // way to detect offline whether the protocol is registered.
  function setupFileButtons() {
    Array.prototype.slice.call(document.querySelectorAll("button.file-btn[data-file]")).forEach(function (button) {
      button.addEventListener("click", function (event) {
        event.stopPropagation();
        var path = button.getAttribute("data-file");
        if (navigator.clipboard && navigator.clipboard.writeText) {
          navigator.clipboard.writeText(path).catch(function () {});
        }
        tryOpenProtocol("apfile://" + encodeURIComponent(path));
        sceneButtonStatus(button, "Copied the path. Opening in your default .cs editor if apfile:// is set up (see assets/tools/register-scene-button.ps1).");
      });
    });
  }

  // Hovering or focusing a node in an interactive diagram traces its connections: lights up the node
  // and every edge touching it, dims everything else. Each node still works as a plain link.
  function setupMapDiagram() {
    Array.prototype.slice.call(document.querySelectorAll("svg.map-diagram")).forEach(function (svg) {
      var nodes = Array.prototype.slice.call(svg.querySelectorAll(".map-node"));
      var edges = Array.prototype.slice.call(svg.querySelectorAll(".map-edge"));

      function clear() {
        svg.classList.remove("diagram-active");
        nodes.forEach(function (n) { n.classList.remove("lit", "dim"); });
        edges.forEach(function (e) { e.classList.remove("lit", "dim"); });
      }

      function light(id) {
        var touching = edges.filter(function (e) {
          return e.getAttribute("data-from") === id || e.getAttribute("data-to") === id;
        });
        var neighborIds = {};
        neighborIds[id] = true;
        touching.forEach(function (e) {
          neighborIds[e.getAttribute("data-from")] = true;
          neighborIds[e.getAttribute("data-to")] = true;
        });
        svg.classList.add("diagram-active");
        nodes.forEach(function (n) {
          var lit = neighborIds[n.getAttribute("data-node")];
          n.classList.toggle("lit", !!lit);
          n.classList.toggle("dim", !lit);
        });
        edges.forEach(function (e) {
          var lit = touching.indexOf(e) !== -1;
          e.classList.toggle("lit", lit);
          e.classList.toggle("dim", !lit);
        });
      }

      nodes.forEach(function (node) {
        var id = node.getAttribute("data-node");
        node.addEventListener("mouseenter", function () { light(id); });
        node.addEventListener("focus", function () { light(id); });
        node.addEventListener("mouseleave", clear);
        node.addEventListener("blur", clear);
      });
    });
  }

  // Clicking a top-level box in a diagram drills into how its own parts connect, shown in a panel
  // below (one open at a time); clicking the open one closes it, clicking another swaps it in.
  function setupDrillDiagram() {
    Array.prototype.slice.call(document.querySelectorAll("svg.map-diagram")).forEach(function (svg) {
      var toggles = Array.prototype.slice.call(svg.querySelectorAll(".drill-toggle"));
      if (toggles.length === 0) {
        return;
      }

      function activate(target) {
        var wasActive = null;
        toggles.forEach(function (t) {
          if (t.classList.contains("active") && t.getAttribute("data-node") === target) {
            wasActive = t;
          }
        });
        toggles.forEach(function (t) {
          var on = !wasActive && t.getAttribute("data-node") === target;
          t.classList.toggle("active", on);
          t.setAttribute("aria-expanded", on ? "true" : "false");
        });
        Array.prototype.slice.call(svg.querySelectorAll(".drill")).forEach(function (g) {
          var show = !wasActive && g.getAttribute("data-for") === target;
          if (show) {
            g.removeAttribute("hidden");
          } else {
            g.setAttribute("hidden", "");
          }
        });
      }

      toggles.forEach(function (toggle) {
        var target = toggle.getAttribute("data-node");
        toggle.addEventListener("click", function () { activate(target); });
        toggle.addEventListener("keydown", function (event) {
          if (event.key === "Enter" || event.key === " ") {
            event.preventDefault();
            activate(target);
          }
        });
      });
    });
  }

  function showBackLink() {
    var params = new URLSearchParams(window.location.search);
    var from = params.get("from");
    var label = params.get("label");
    var main = document.querySelector("main");
    if (!from || !main) {
      return;
    }
    var back = el("a", { class: "back", href: from }, "&larr; Back to " + escapeHtml(label || "the guide"));
    main.insertBefore(back, main.firstChild);
  }

  document.addEventListener("DOMContentLoaded", function () {
    buildTree();
    buildCrumbs();
    addHeadingAnchors();
    buildToc();
    buildPager();
    highlightCode();
    addCopyButtons();
    setupGlossaryTooltips();
    tagSampleLinks();
    showBackLink();
    setupSceneButtons();
    setupFileButtons();
    setupMapDiagram();
    setupDrillDiagram();
    buildHistoryTrail();
  });
})();
