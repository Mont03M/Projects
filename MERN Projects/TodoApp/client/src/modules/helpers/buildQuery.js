export const BuildQuery = (filter) => {
  const params = new URLSearchParams();

  Object.entries(filter).forEach(([key, value]) => {
    if (value) {
      params.append(key, value);
    }
  });

  return params.toString() ? `?${params.toString()}` : "";
};
