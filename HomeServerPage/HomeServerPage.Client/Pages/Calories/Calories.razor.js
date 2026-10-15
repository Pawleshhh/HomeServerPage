const pad = value => String(value).padStart(2, "0");

const formatLocalDate = date =>
    `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;

const formatLocalTime = date =>
    `${pad(date.getHours())}:${pad(date.getMinutes())}`;

export const getLocalClock = () => {
    const now = new Date();
    return {
        date: formatLocalDate(now),
        time: formatLocalTime(now)
    };
};

export const getOffsetMinutes = (dateValue, timeValue) => {
    const [year, month, day] = dateValue.split("-").map(Number);
    const [hour, minute] = timeValue.split(":").map(Number);
    const localDateTime = new Date(year, month - 1, day, hour, minute, 0, 0);
    return -localDateTime.getTimezoneOffset();
};

window.homeServerPageCalories = { getLocalClock, getOffsetMinutes };
