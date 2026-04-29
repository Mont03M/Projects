export const KeywordSearch = (elements, params) => {
  if (!params) return [];

  const keywords = (Array.isArray(params) ? params : (params || "").split(" "))
    .map((p) => p.toLowerCase())
    .filter(Boolean);

  const matches = elements.filter((element) => {
    const values = Object.values(element || {})
      .join(" ")
      .toLocaleLowerCase();

    return keywords.every((kw) => values.includes(kw));
  });

  return matches || [];
};
