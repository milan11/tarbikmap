export function formatNumber(num: number) {
  return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, " ");
}

export function formatDistance(num: number) {
  if (num >= 10) {
    return formatNumber(Math.floor(num)) + " km";
  }

  if (num >= 1) {
    return Math.floor(num * 10) / 10 + " km";
  }

  return Math.floor(num * 1000) + " m";
}
