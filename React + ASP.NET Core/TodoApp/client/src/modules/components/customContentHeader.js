import { Container } from "react-bootstrap";

export function CustomContentHeader({ children }) {
  return (
    <>
      <Container fluid>
        <div className="border rounded p-3 bg-white w-100 overflow-hidden">
          {children}
        </div>
      </Container>
    </>
  );
}
