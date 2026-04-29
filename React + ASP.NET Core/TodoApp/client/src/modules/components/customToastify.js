import { useState } from "react";
import Toast from "react-bootstrap/Toast";
import ToastContainer from "react-bootstrap/ToastContainer";

export function ToastMessage({ show, onClose, header, message }) {
  return (
    <>
      <ToastContainer position="top-end" className="p-3">
        <Toast show={show} onClose={onClose} autohide>
          <Toast.Header>
            <strong className="me-auto">{header}</strong>
          </Toast.Header>
          <Toast.Body>{message}</Toast.Body>
        </Toast>
      </ToastContainer>
    </>
  );
}
