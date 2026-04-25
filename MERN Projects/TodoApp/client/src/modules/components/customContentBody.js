import { Container } from "react-bootstrap";

export function CustomContentBody({ children }) {
  return (
    <>
      <Container fluid>
        <div
          className="border rounded p-3 bg-white w-100 overflow-hidden"
          style={{ minHeight: "500px" }}
        >
          <div className="d-flex flex-column gap-3 text-break w-100">
            {children}
          </div>
        </div>
      </Container>
    </>
  );
}
