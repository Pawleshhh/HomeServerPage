const clock = document.querySelector(".tablet-navigation-clock[data-server-time]");

if (clock) {
    function startClock() {
        const startTime = new Date(clock.dataset.serverTime);
        if (Number.isNaN(startTime.getTime())) {
            return false;
        }

        const browserTimeAtStart = Date.now();
        const dateElement = clock.querySelector(".tablet-navigation-date");
        const timeElement = clock.querySelector(".tablet-navigation-time");

        function updateClock() {
            const currentTime = new Date(startTime.getTime() + Date.now() - browserTimeAtStart);
            dateElement.textContent = currentTime.toLocaleDateString();
            timeElement.textContent = currentTime.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit", second: "2-digit", hour12: false });
            setTimeout(updateClock, 1000 - currentTime.getMilliseconds());
        }

        updateClock();
        return true;
    }

    if (!startClock()) {
        const observer = new MutationObserver(() => {
            if (startClock()) {
                observer.disconnect();
            }
        });

        observer.observe(clock, { attributes: true, attributeFilter: ["data-server-time"] });
    }
}