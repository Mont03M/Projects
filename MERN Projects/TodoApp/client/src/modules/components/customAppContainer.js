import { Container } from "react-bootstrap";

export function CustomAppContainer({ children }) {
  return (
    <>
      <Container fluid className="p-3">
        <div className="d-flex flex-wrap gap-2 text-break">{children}</div>
      </Container>
    </>
  );
}
