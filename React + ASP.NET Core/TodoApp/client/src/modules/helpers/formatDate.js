export const FormatDate = (date) => {
  return String(date.split("T").filter(Boolean)[0]);
};
