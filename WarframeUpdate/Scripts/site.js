/*  ═══════════════════════════════════════════
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
    let shouldRefresh = false;

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

        // Mark for refresh when timer expires
        if (remaining <= 0 && !el.dataset.expired) {
            el.dataset.expired = '1';
            shouldRefresh = true;
        }
    });

    // Refresh data when any timer expires
    if (shouldRefresh) {
        setTimeout(() => {
            refreshDashboardData();
        }, 1000);
    }
}

// ── Refresh Dashboard Data (Full Page Update) ────────────
async function refreshDashboardData() {
    try {
        console.log('[Dashboard] Fetching fresh data...');
        const response = await fetch(window.location.href, {
            method: 'GET',
            headers: {
                'Accept': 'text/html'
            }
        });

        if (!response.ok) {
            console.error('[Dashboard] HTTP error:', response.status);
            return;
        }

        const html = await response.text();
        
        // Parse the new HTML
        const parser = new DOMParser();
        const newDoc = parser.parseFromString(html, 'text/html');
        
        // Update dashboard content
        const newDashboard = newDoc.querySelector('.dashboard');
        const currentDashboard = document.querySelector('.dashboard');
        
        if (newDashboard && currentDashboard) {
            currentDashboard.innerHTML = newDashboard.innerHTML;
            console.log('[Dashboard] Dashboard updated');
            
            // Reset expired flags on all countdowns
            document.querySelectorAll('.countdown[data-expiry]').forEach(el => {
                el.dataset.expired = '0';
            });
            
            // Update world state bar if it exists
            const newWorldState = newDoc.querySelector('.world-state-bar');
            const currentWorldState = document.querySelector('.world-state-bar');
            if (newWorldState && currentWorldState) {
                currentWorldState.innerHTML = newWorldState.innerHTML;
                console.log('[Dashboard] World state bar updated');
            }
            
            // Re-init animations on new cards
            initAnimations();
            initFissureShowMore();
            
            // Trigger immediate countdown update
            tickCountdowns();
            tickCycleTimers();
            
            console.log('[Dashboard] UI fully updated');
        }
    } catch (error) {
        console.error('[Dashboard] Refresh failed:', error);
    }
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

// ── Show More/Less for Fissures ──────────────────────────
function initFissureShowMore() {
    const showMoreBtn = document.getElementById('fissureShowMore');
    
    if (showMoreBtn) {
        showMoreBtn.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            
            const container = document.querySelector('.fissures-container');
            
            if (!container) {
                console.error('Fissures container not found');
                return;
            }
            
            const hiddenRows = container.querySelectorAll('.fissure-hidden');
            
            if (hiddenRows.length > 0) {
                // Show all hidden rows
                hiddenRows.forEach(row => row.classList.remove('fissure-hidden'));
                showMoreBtn.textContent = 'SHOW LESS';
                showMoreBtn.classList.add('collapsed');
                showMoreBtn.dataset.expanded = 'true';
            } else {
                // Hide rows again (show only first 5)
                const itemsToShowInitially = 5;
                const allRows = container.querySelectorAll('.fissure-row');
                
                allRows.forEach((row, index) => {
                    if (index >= itemsToShowInitially) {
                        row.classList.add('fissure-hidden');
                    }
                });
                
                const hiddenCount = allRows.length - itemsToShowInitially;
                showMoreBtn.textContent = `SHOW MORE (${hiddenCount} MORE)`;
                showMoreBtn.classList.remove('collapsed');
                showMoreBtn.dataset.expanded = 'false';
            }
        });
    }
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
    initFissureShowMore();

    // Tick every second
    setInterval(() => {
        updateServerTime();
        tickCountdowns();
        tickCycleTimers();
    }, 1000);
});
