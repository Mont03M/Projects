import { Badge } from "react-bootstrap";

export function CustomBadgeGroup({ bgArray, BgColor, removeHandler }) {
  return (
    <>
      {/** Badges */}
      <div className="d-flex justify-content-center">
        {Array.isArray(bgArray) && bgArray.length > 0 && (
          <p className="mb-0 text-center">
            Filter(s):
            {bgArray.map(
              (item, index) =>
                item && (
                  <Badge key={index} bg={BgColor(item.value)} className="ms-1">
                    <span
                      key={index}
                      style={{ cursor: "pointer", fontWeight: "bold" }}
                      onClick={() => removeHandler(item, index)}
                    >
                      ×
                    </span>
                    {item.value}
                  </Badge>
                ),
            )}
          </p>
        )}
      </div>
    </>
  );
}
