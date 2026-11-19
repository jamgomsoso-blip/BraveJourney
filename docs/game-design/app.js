(() => {
  const groups = Array.from(document.querySelectorAll('[data-tabs]')).map(group => {
    const bar = group.firstElementChild;
    const controls = Array.from(bar.querySelectorAll('a[href^="#"]'));
    const panels = controls.map(link => document.getElementById(link.hash.slice(1)));
    function activate(index) {
      controls.forEach((control, position) => {
        const selected = position === index;
        control.setAttribute('aria-selected', String(selected));
        control.tabIndex = selected ? 0 : -1;
        panels[position].hidden = !selected;
      });
    }
    bar.setAttribute('role', 'tablist');
    controls.forEach((control, index) => {
      control.id = 'tab-' + panels[index].id;
      control.setAttribute('role', 'tab');
      control.setAttribute('aria-controls', panels[index].id);
      panels[index].setAttribute('role', 'tabpanel');
      panels[index].setAttribute('aria-labelledby', control.id);
      panels[index].tabIndex = 0;
      control.addEventListener('click', event => {
        if (event.ctrlKey || event.metaKey || event.altKey || event.shiftKey) return;
        event.preventDefault();
        activate(index);
        if (location.hash !== control.hash) history.pushState(null, '', control.hash);
      });
      control.addEventListener('keydown', event => {
        let next;
        if (event.key === 'ArrowRight') next = (index + 1) % controls.length;
        else if (event.key === 'ArrowLeft') next = (index - 1 + controls.length) % controls.length;
        else if (event.key === 'Home') next = 0;
        else if (event.key === 'End') next = controls.length - 1;
        else return;
        event.preventDefault();
        controls[next].focus();
        controls[next].click();
      });
    });
    activate(0);
    return {controls, activate};
  });
  function revealAddress() {
    for (const {controls, activate} of groups) {
      const index = controls.findIndex(control => control.hash === location.hash);
      activate(index !== -1 ? index : 0);
    }
  }
  revealAddress();
  window.addEventListener('hashchange', revealAddress);
  window.addEventListener('popstate', revealAddress);
})();

(() => {
  const control = document.querySelector('.sim-step');
  const stages = [
    ['05:59:59 · 미션 완료', '완료·미수령 상태입니다. ‘받기’를 누를 수 있습니다.', '이전 회차의 완료 시각과 지급 ID를 기록하고 발송을 대기합니다.', '06:00 초기화 적용'],
    ['06:00:00 · 회차 초기화', '이전 회차의 미수령 보상이 사라집니다.', '새 회차가 시작돼도 이전 회차의 발송 대기 건을 유지합니다.', '발송 지연 후 재접속'],
    ['재접속 · 서버 발송 확인', '이전 회차의 보상을 다시 받을 수 없습니다.', '같은 지급 ID로 한 번만 우편을 보냅니다. 우편을 열어 꺼내야 실제 수령입니다.', '처음부터 다시 보기']
  ];
  let stage = 0;
  control.hidden = false;
  control.addEventListener('click', () => {
    stage = (stage + 1) % stages.length;
    const [clock, current, proposed, action] = stages[stage];
    document.getElementById('clock').textContent = clock;
    document.getElementById('old-state').textContent = current;
    document.getElementById('new-state').textContent = proposed;
    control.textContent = action;
  });
})();
