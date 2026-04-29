import { CustomAppContainer } from "../../components/customAppContainer";
import { CustomContentHeader } from "../../components/customContentHeader";
import { CustomContentBody } from "../../components/customContentBody";
import { CustomSearchBar } from "../../components/customSearchBar";
import { CustomBadgeGroup } from "../../components/customBadgeGroup";
import { CustomFilterForm } from "../../components/customFilterForm";
import { useState, useEffect } from "react";
import { useForm, useFieldArray } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { FormatDate } from "../../helpers/formatDate";
import { KeywordSearch } from "../../helpers/keywordSearch";
import { useTodos } from "../../contexts/todoContext";
import { BgFilterColor } from "../../helpers/bgFilterColor";
import { AppRoutes } from "./_config";
import { API } from "../../services/apiService";
import { ToastMessage } from "../../components/customToastify";
import {
  Badge,
  Tooltip,
  Spinner,
  Button,
  Dropdown,
  Card,
  Stack,
  OverlayTrigger,
} from "react-bootstrap";

export function TodoApp() {
  const { todos, loading, error, getTodos } = useTodos();
  const [showFilter, setShowFilter] = useState(false);
  const [bgFilter, setBgFilter] = useState([]);
  const [cacheTodos, setCacheTodos] = useState([]);
  const [toast, setToast] = useState({
    show: false,
    header: "",
    message: "",
  });

  const navigate = useNavigate();

  const form = useForm({
    defaultValues: {
      searchParams: "",
      filter: {
        status: "",
        priority: "",
        dueDate: "",
      },
      todos: [],
    },
  });

  const { control, setValue, getValues, reset } = form;

  const {
    fields: todoFields,
    remove: todoRemove,
    replace: todosReplace,
  } = useFieldArray({ control, name: "todos" });

  const handleFilterTodo = (filter) => {
    // filter object
    const filterObject = Object.entries(filter || {})
      .filter(([_, value]) => value && `${value}`.trim() !== "")
      .map(([key, value]) => ({ key, value }));

    // filter object empty
    if (!filterObject) return;

    // set badge filter
    setBgFilter(filterObject);

    // filter backend
    API.post(
      `/${AppRoutes.relativeRoute}/${AppRoutes.endpoints.filter}`,
      filter,
    )
      .then((response) => {
        if (response?.data?.success && response?.data?.data) {
          const matches = response.data.data;
          todosReplace(matches);
          setCacheTodos(matches);
        }
      })
      .catch((err) => console.error(err));
  };

  const handleRemoveSingleFilter = (item, index) => {
    const updatedFilters = bgFilter.filter((_, i) => i !== index);

    // set badge filter
    setBgFilter(updatedFilters);

    // re-run filtering
    if (updatedFilters.length > 0) {
      const filterList = updatedFilters.map((f) => f.value);
      const matches = KeywordSearch(todos, filterList);
      todosReplace(matches);
      setCacheTodos(matches);
      // reset removed filter
      reset({
        filter: {
          [item.key]: "",
        },
      });
    } else {
      // all filters removed -- reset
      handleRemoveFilter();
    }
  };

  const handleRemoveFilter = () => {
    reset({
      filter: {
        status: "",
        priority: "",
        dueDate: "",
      },
    });
    setBgFilter(null);
    todosReplace(todos);
  };

  const handleSearch = (params) => {
    const todoSet = bgFilter && bgFilter.length > 0 ? cacheTodos : todos;

    // empty entry
    if (!params || params.trim() === "") {
      todosReplace(todoSet);
      return;
    }

    // params as array
    params = params.split(" ").filter(Boolean);

    // keyword matches
    let matches = KeywordSearch(todoSet, params);

    if (matches) {
      todosReplace(matches);
    }
  };

  const resetSearchParams = () => {
    if (getValues("searchParams")) {
      const todoSet = bgFilter && bgFilter.length > 0 ? cacheTodos : todos;
      setValue("searchParams", "");
      todosReplace(todoSet);
    }
  };

  const handleCreateTodo = () => {
    navigate(`/${AppRoutes.baseRoute}/add`);
  };

  const handleViewTodo = (todo) => {
    if (!todo || !todo?._id) return;
    navigate(`/${AppRoutes.baseRoute}/view/` + todo._id, {
      state: todo,
    });
  };

  const handleEditTodo = (todo) => {
    if (!todo || !todo?._id) return;
    navigate(`/${AppRoutes.baseRoute}/edit/` + todo._id, {
      state: todo,
    });
  };

  const handleDeleteTodo = (todo, index) => {
    if (!todo || !todo?._id) return;

    // delete todo
    API.delete(`/${AppRoutes.relativeRoute}/` + todo._id)
      .then((response) => {
        if (response?.data?.success) {
          todoRemove(index);

          setToast({
            show: true,
            header: "Deleted!",
            message: `Todo ${todo.name} was successfully deleted!`,
          });
        }
      })
      .catch((err) => {
        console.error(err.message);
      });
  };

  useEffect(() => {
    getTodos();
  }, []);

  useEffect(() => {
    if (todos && todos.length) {
      todosReplace(todos);
    }
  }, [todos]);

  if (error) {
    return (
      <div className="d-flex flex-column justify-content-center align-items-center vh-100">
        <i
          className="bi bi-x-circle-fill text-danger"
          style={{ fontSize: "4rem" }}
        />
        <h4 className="mt-3 text-danger">Error loading todos!</h4>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center vh-100">
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
      </div>
    );
  }

  return (
    <>
      <CustomAppContainer>
        <CustomContentHeader>
          <h1 className="text-center w-100">Todo App</h1>
        </CustomContentHeader>
        <CustomContentBody>
          <div
            className="w-100 p-2 border rounded border-gray d-flex align-items-center flex-wrap gap-3"
            style={{ minWidth: "300px" }}
          >
            {/** Create  */}
            <Button onClick={() => handleCreateTodo()} variant="primary">
              Create
            </Button>
            <Dropdown
              className="ps-2"
              show={showFilter}
              onToggle={(isOpen) => setShowFilter(isOpen)}
            >
              {/** filter form */}
              <Dropdown.Toggle variant="secondary" id="filter-dropdown">
                Filter
              </Dropdown.Toggle>
              <Dropdown.Menu className="p-3" style={{ minWidth: "250px" }}>
                {/** filter */}
                <CustomFilterForm
                  form={form}
                  filterHandler={handleFilterTodo}
                  removeHandler={handleRemoveFilter}
                  showForm={showFilter}
                  setShow={setShowFilter}
                />
              </Dropdown.Menu>
            </Dropdown>
            {/** filter badges */}
            <CustomBadgeGroup
              bgArray={bgFilter}
              BgColor={BgFilterColor}
              removeHandler={handleRemoveSingleFilter}
            />
            <Badge bg="dark">Total Count: {todoFields.length}</Badge>
            {/** SEARCH */}
            <CustomSearchBar
              form={form}
              resetSearchParams={resetSearchParams}
              searchHandler={handleSearch}
            />
          </div>
          {/** App content body */}
          <Stack
            direction="horizontal"
            gap={5}
            className="w-100 p-2 d-flex align-items-center flex-wrap gap-3 ps-5"
          >
            {/** list of todos */}
            {todoFields && todoFields.length > 0 ? (
              todoFields.map((todo, index) => (
                <div
                  key={todo._id}
                  className="border p-2 d-flex border rounded align-items-center gap-3"
                  style={{ maxWidth: "530px" }}
                >
                  {/* Buttons */}
                  <Stack gap={1}>
                    {/**Overlay edit */}
                    <OverlayTrigger
                      placement="top"
                      overlay={<Tooltip id="edit-tooltip">Edit Todo</Tooltip>}
                    >
                      {/**edit button */}
                      <Button
                        variant="warning"
                        onClick={() => handleEditTodo(todo)}
                      >
                        <i className="bi bi-pencil"></i>
                      </Button>
                    </OverlayTrigger>

                    {/** Overlay view */}
                    <OverlayTrigger
                      placement="top"
                      overlay={<Tooltip id="view-tooltip">View Todo</Tooltip>}
                    >
                      {/** view button */}
                      <Button
                        variant="info"
                        onClick={() => handleViewTodo(todo)}
                      >
                        <i className="bi bi-eye"></i>
                      </Button>
                    </OverlayTrigger>
                    {/** overlay delete */}
                    <OverlayTrigger
                      placement="top"
                      overlay={
                        <Tooltip id="delete-tooltip">Delete Todo</Tooltip>
                      }
                    >
                      {/** delete button */}
                      <Button
                        variant="danger"
                        onClick={() => handleDeleteTodo(todo, index)}
                      >
                        <i className="bi bi-trash3"></i>
                      </Button>
                    </OverlayTrigger>
                  </Stack>
                  {/* Card */}
                  <Card>
                    <Card.Body className="d-flex align-items-center justify-content-between flex-wrap gap-3">
                      {/* LEFT: Title + Subtitle */}
                      <div>
                        <Card.Title className="mb-0">{todo.name}</Card.Title>
                        <Card.Subtitle className="text-muted">
                          {todo.task}
                        </Card.Subtitle>
                      </div>
                      {/* Bottom Detials */}
                      <div className="d-flex gap-3 flex-wrap">
                        <span>
                          <strong>Status: </strong>
                          <Badge bg={BgFilterColor(todo.status)}>
                            {todo.status}
                          </Badge>
                        </span>
                        <span>
                          <strong>Priority: </strong>
                          <Badge bg={BgFilterColor(todo.priority)}>
                            {todo.priority}
                          </Badge>
                        </span>
                        <span>
                          <strong>Due: </strong>
                          <Badge bg={BgFilterColor(FormatDate(todo.dueDate))}>
                            {FormatDate(todo.dueDate)}
                          </Badge>
                        </span>
                      </div>
                    </Card.Body>
                  </Card>
                </div>
              ))
            ) : (
              <div className="w-100 d-flex justify-content-center align-items-center p-5 text-muted">
                <h2 className="m-0">No Todos Available!</h2>
              </div>
            )}
          </Stack>
          {/**Delete Message */}
          <ToastMessage
            show={toast.show}
            header={toast.header}
            message={toast.message}
            onClose={() => setToast({ ...toast, show: false })}
          />
        </CustomContentBody>
      </CustomAppContainer>
    </>
  );
}
