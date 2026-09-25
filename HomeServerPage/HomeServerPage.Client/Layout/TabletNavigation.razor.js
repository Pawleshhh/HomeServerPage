const root = document.body ?? document.documentElement;
const observer = new MutationObserver(syncClock);
let activeClock;
let activeTimeElement;
let activeServerTime;
let timeoutId;

function syncClock() {
    const clock = document.querySelector(".tablet-navigation-clock[data-server-time]");
    const timeElement = clock?.querySelector(".tablet-navigation-time");

    if (!clock || !timeElement) {
        stopClock();
        return;
    }

    const serverTime = clock.dataset.serverTime;
    if (clock === activeClock && timeElement === activeTimeElement && serverTime === activeServerTime) {
        return;
    }

    const startTime = new Date(serverTime);
    if (Number.isNaN(startTime.getTime())) {
        stopClock();
        return;
    }

    stopClock();
    activeClock = clock;
    activeTimeElement = timeElement;
    activeServerTime = serverTime;

    const browserTimeAtStart = Date.now();
    function updateClock() {
        if (timeElement !== activeTimeElement) {
            return;
        }

        const currentTime = new Date(startTime.getTime() + Date.now() - browserTimeAtStart);
        timeElement.textContent = currentTime.toLocaleTimeString([], {
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit",
            hour12: false
        });
        timeoutId = setTimeout(updateClock, 1000 - currentTime.getMilliseconds());
    }

    updateClock();
}

function stopClock() {
    clearTimeout(timeoutId);
    timeoutId = undefined;
    activeClock = undefined;
    activeTimeElement = undefined;
    activeServerTime = undefined;
}

syncClock();
observer.observe(root, { childList: true, subtree: true, attributes: true, attributeFilter: ["data-server-time"] });
