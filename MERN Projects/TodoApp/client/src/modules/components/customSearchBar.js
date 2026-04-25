import { InputGroup, Form, Button } from "react-bootstrap";

export function CustomSearchBar({ form, resetSearchParams, searchHandler }) {
  const { register } = form;

  return (
    <>
      {/** SEARCH */}
      <form className="ms-md-auto" style={{ minWidth: "370px" }}>
        <InputGroup className="w-100">
          <Button variant="secondary" onClick={resetSearchParams}>
            clear
          </Button>
          <Form.Control
            {...register("searchParams")}
            placeholder="Search..."
            onChange={(e) => searchHandler(e.target.value)}
          />
        </InputGroup>
      </form>
    </>
  );
}
