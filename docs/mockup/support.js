// Minimal offline renderer for the mockup .dc.html files (exported from Claude Design).
// shortcut: supports only {{path}}, <sc-for>, <sc-if>, <helmet> and renderVals() – enough for these static screens.
class DCLogic { constructor() { this.props = {}; this.state = {}; } }
window.DCLogic = DCLogic;

document.addEventListener('DOMContentLoaded', () => {
  const src = document.querySelector('script[data-dc-script]').textContent;
  const Component = new Function('DCLogic', src + '\nreturn Component;')(DCLogic);
  const vals = new Component().renderVals();
  const root = document.querySelector('x-dc');

  const resolve = (path, scope) => path.trim().split('.').reduce((o, k) => (o == null ? undefined : o[k]), scope);
  const fill = (s, scope) => s.replace(/\{\{\s*([^}]+?)\s*\}\}/g, (_, p) => { const v = resolve(p, scope); return v == null ? '' : v; });

  function render(node, scope) {
    if (node.nodeType === Node.TEXT_NODE) return [document.createTextNode(fill(node.textContent, scope))];
    if (node.nodeType !== Node.ELEMENT_NODE) return [];
    const tag = node.tagName.toLowerCase();
    const kids = (sc) => [...node.childNodes].flatMap((c) => render(c, sc));
    if (tag === 'sc-for') {
      const list = resolve(node.getAttribute('list').replace(/[{}]/g, ''), scope) || [];
      const as = node.getAttribute('as');
      return list.flatMap((item, i) => kids({ ...scope, [as]: item, $index: i }));
    }
    if (tag === 'sc-if') return resolve(node.getAttribute('value').replace(/[{}]/g, ''), scope) ? kids(scope) : [];
    if (tag === 'helmet') { kids(scope).forEach((n) => document.head.appendChild(n)); return []; }
    const el = document.createElementNS(node.namespaceURI, node.localName);
    for (const a of node.attributes) el.setAttribute(a.name, fill(a.value, scope));
    kids(scope).forEach((n) => el.appendChild(n));
    return [el];
  }

  const out = [...root.childNodes].flatMap((c) => render(c, vals));
  root.replaceWith(...out);
});
