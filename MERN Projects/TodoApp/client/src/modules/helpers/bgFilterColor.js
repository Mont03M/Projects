export function BgFilterColor(
  value,
  bgColors = {
    Completed: "success",
    Pending: "warning",
    High: "danger",
    Medium: "warning",
    Low: "info",
    dueDate: "primary",
  },
) {
  const isDate = /^\d{4}-\d{2}-\d{2}$/.test(value);
  if (isDate) return bgColors.dueDate;

  return bgColors[value] || [];
}
