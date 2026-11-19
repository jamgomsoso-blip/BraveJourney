(() => {
  const viewer = document.createElement('dialog');
  viewer.id = 'photo-viewer';
  viewer.setAttribute('aria-label', '사진 크게 보기');
  viewer.innerHTML = `<div class="photo-tools"><button type="button" class="photo-zoom" aria-pressed="false">2배 확대</button><button type="button" class="photo-close" autofocus>닫기</button></div><div class="photo-stage"><div class="photo-surface"></div></div><p class="photo-note" id="photo-description"></p>`;
  viewer.setAttribute('aria-describedby', 'photo-description');
  document.body.append(viewer);
  const stage = viewer.querySelector('.photo-stage');
  const surface = viewer.querySelector('.photo-surface');
  const note = viewer.querySelector('.photo-note');
  const zoom = viewer.querySelector('.photo-zoom');
  const close = viewer.querySelector('.photo-close');
  const badgeIcon = '<svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="10" cy="10" r="6.5"/><path d="m15 15 6 6M7 10h6m-3-3v6"/></svg>';
  const composite = '.mine-display > div';
  const candidates = 'img,' + composite;
  let opener = null;
  let ratio = 1;
  let enlarged = false;

  function description(item) {
    return (item.closest('figure')?.querySelector('figcaption')?.textContent || item.getAttribute('alt') || '').trim();
  }

  function prepare(item) {
    if (item.closest('#photo-viewer,a,button,summary,[aria-hidden="true"],[role="tab"]')) return;
    const group = item.matches(composite);
    if (!group && (!item.alt?.trim() || item.closest(composite))) return;
    const label = description(item);
    item.dataset.photoOpen = '';
    item.tabIndex = 0;
    item.setAttribute('role', 'button');
    item.setAttribute('aria-haspopup', 'dialog');
    item.setAttribute('aria-label', (item.alt || label) + ' 크게 보기');
    const host = group ? item : item.parentElement;
    if (!host.matches('figure,.quest-shot,.hud-monitor,.heir-portrait,.portrait,.mine-display > div') || host.querySelector(':scope > .photo-badge')) return;
    if (getComputedStyle(host).position === 'static') host.classList.add('photo-badge-host');
    const badge = document.createElement('span');
    badge.className = 'photo-badge';
    badge.setAttribute('aria-hidden', 'true');
    badge.innerHTML = badgeIcon;
    host.append(badge);
  }

  function scan(root) {
    if (!(root instanceof Element) || root.closest('#photo-viewer')) return;
    if (root.matches(candidates)) prepare(root);
    root.querySelectorAll(candidates).forEach(prepare);
  }

  document.querySelectorAll(candidates).forEach(prepare);
  new MutationObserver(changes => {
    for (const change of changes) {
      if (change.type === 'attributes') prepare(change.target);
      else change.addedNodes.forEach(scan);
    }
  }).observe(document.body, {childList: true, subtree: true, attributes: true, attributeFilter: ['alt']});

  function fit() {
    if (!viewer.open || !stage.clientWidth || !stage.clientHeight) return;
    const width = Math.min(stage.clientWidth, stage.clientHeight * ratio) * (enlarged ? 2 : 1);
    surface.style.width = width + 'px';
    surface.style.height = width / ratio + 'px';
  }

  function open(item) {
    if (viewer.open) return;
    const single = item instanceof HTMLImageElement;
    const originals = single ? [item] : Array.from(item.querySelectorAll('img'));
    if (!originals.length || originals.some(img => !(img.currentSrc || img.src))) return;
    const frame = item.getBoundingClientRect();
    ratio = single && item.naturalHeight ? item.naturalWidth / item.naturalHeight : frame.width / Math.max(1,frame.height);
    opener = item;
    note.textContent = description(item);
    enlarged = false;
    zoom.textContent = '2배 확대';
    zoom.setAttribute('aria-pressed','false');
    for (const source of originals) {
      const image = new Image();
      image.alt = single ? (source.alt || note.textContent) : '';
      if (single) {
        image.onload = () => {
          if (image.parentNode === surface && image.naturalHeight) {
            ratio = image.naturalWidth / image.naturalHeight;
            fit();
          }
        };
      } else {
        const box = source.getBoundingClientRect();
        const css = getComputedStyle(source);
        Object.assign(image.style, {
          inset: 'auto', left: (box.left-frame.left)/frame.width*100+'%', top: (box.top-frame.top)/frame.height*100+'%',
          width: box.width/frame.width*100+'%', height: box.height/frame.height*100+'%',
          objectFit: css.objectFit, objectPosition: css.objectPosition
        });
      }
      image.src = source.currentSrc || source.src;
      surface.append(image);
    }
    viewer.showModal();
    document.documentElement.classList.add('photo-view-open');
    fit();
    stage.scrollTo(0,0);
    close.focus({preventScroll:true});
  }

  function toggleZoom() {
    enlarged = !enlarged;
    zoom.textContent = enlarged ? '화면에 맞추기' : '2배 확대';
    zoom.setAttribute('aria-pressed', String(enlarged));
    fit();
    stage.scrollTo((stage.scrollWidth-stage.clientWidth)/2,(stage.scrollHeight-stage.clientHeight)/2);
  }

  document.addEventListener('click', event => {
    const item = event.target.closest('[data-photo-open]');
    if (item) open(item);
  });
  document.addEventListener('keydown', event => {
    const item = event.target.closest('[data-photo-open]');
    if (item && !event.repeat && (event.key === 'Enter' || event.key === ' ')) {
      event.preventDefault();
      open(item);
    }
  });
  zoom.addEventListener('click', toggleZoom);
  close.addEventListener('click', () => viewer.close());
  stage.addEventListener('click', event => { if (event.target === stage) viewer.close(); });
  viewer.addEventListener('close', () => {
    document.documentElement.classList.remove('photo-view-open');
    if (opener?.isConnected) opener.focus({preventScroll:true});
    opener = null;
    surface.replaceChildren();
  });
  new ResizeObserver(fit).observe(stage);
})();
