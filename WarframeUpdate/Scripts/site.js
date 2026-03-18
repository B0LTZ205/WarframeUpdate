/* ═══════════════════════════════════════════
   WARFRAME TRACKER — CLIENT SCRIPT
   ═══════════════════════════════════════════ */

// ── Server Clock ──────────────────────────────────────────
function updateServerTime() {
    const el = document.getElementById('server-time');
    if (!el) return;
    const now = new Date();
    const h = String(now.getUTCHours()).padStart(2, '0');
    const m = String(now.getUTCMinutes()).padStart(2, '0');
    const s = String(now.getUTCSeconds()).padStart(2, '0');
    el.textContent = `${h}:${m}:${s} UTC`;
}

// ── Countdown Timers ──────────────────────────────────────
function formatRemaining(ms) {
    if (ms <= 0) return '00:00:00';
    const totalSec = Math.floor(ms / 1000);
    const d = Math.floor(totalSec / 86400);
    const h = Math.floor((totalSec % 86400) / 3600);
    const m = Math.floor((totalSec % 3600) / 60);
    const s = totalSec % 60;

    if (d > 0) return `${d}d ${String(h).padStart(2, '0')}h ${String(m).padStart(2, '0')}m`;
    return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
}

function tickCountdowns() {
    const now = Date.now();
    document.querySelectorAll('.countdown[data-expiry]').forEach(el => {
        const expiry = new Date(el.dataset.expiry).getTime();
        const remaining = expiry - now;
        el.textContent = formatRemaining(remaining);

        // Turn red when under 30 minutes
        if (remaining < 30 * 60 * 1000 && remaining > 0) {
            el.classList.add('urgent');
        } else {
            el.classList.remove('urgent');
        }

        // Auto-reload page when something expires
        if (remaining <= 0 && !el.dataset.expired) {
            el.dataset.expired = '1';
            setTimeout(() => location.reload(), 3000);
        }
    });
}

// ── Cycle Timers ─────────────────────────────────────────
function tickCycleTimers() {
    const now = Date.now();
    document.querySelectorAll('.cycle-timer[data-expiry]').forEach(el => {
        const expiry = new Date(el.dataset.expiry).getTime();
        const remaining = expiry - now;
        if (remaining > 0) {
            const totalSec = Math.floor(remaining / 1000);
            const m = Math.floor(totalSec / 60);
            const s = totalSec % 60;
            el.textContent = `${m}:${String(s).padStart(2, '0')}`;
        } else {
            el.textContent = 'ENDING...';
        }
    });
}

// ── Last Updated Timestamp ───────────────────────────────
function setUpdateTime() {
    const el = document.getElementById('update-time');
    if (!el) return;
    const now = new Date();
    el.textContent = now.toLocaleTimeString();
}

// ── Staggered Card Entrance ──────────────────────────────
function initAnimations() {
    const cards = document.querySelectorAll('.card');
    cards.forEach((card, i) => {
        card.style.animationDelay = `${0.05 * i}s`;
    });
}

// ── Smooth Nav Highlight ─────────────────────────────────
function initNavHighlight() {
    const sections = document.querySelectorAll('section[id]');
    const navLinks = document.querySelectorAll('.nav-link');

    const observer = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                navLinks.forEach(link => {
                    link.style.color = '';
                    link.style.borderColor = '';
                    link.style.background = '';
                });
                const active = document.querySelector(`.nav-link[href="#${entry.target.id}"]`);
                if (active) {
                    active.style.color = 'var(--accent)';
                    active.style.borderColor = 'var(--accent-dim)';
                    active.style.background = 'var(--accent-glow)';
                }
            }
        });
    }, { threshold: 0.3 });

    sections.forEach(s => observer.observe(s));
}

// ── Auto Refresh (every 5 min) ───────────────────────────
function initAutoRefresh() {
    setTimeout(() => {
        // Only reload if page is visible to avoid waking sleeping tabs
        if (!document.hidden) {
            location.reload();
        }
    }, 5 * 60 * 1000);
}

// ── Init ─────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    // Run immediately
    updateServerTime();
    tickCountdowns();
    tickCycleTimers();
    setUpdateTime();
    initAnimations();
    initNavHighlight();
    initAutoRefresh();

    // Tick every second
    setInterval(() => {
        updateServerTime();
        tickCountdowns();
        tickCycleTimers();
    }, 1000);
});
