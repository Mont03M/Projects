const todoController = require("../controllers/todoController");
const express = require("express");
const router = express.Router();

router
  .route("/list")
  .get(todoController.getList)
  .post(todoController.deleteOne);

router.route("/search").post(todoController.searchAll);

router
  .route("/:id")
  .get(todoController.getOne)
  .patch(todoController.updateOne)
  .put(todoController.deleteOneById);

router.route("/bulk-update").post(todoController.updateMany);

router.route("/").get(todoController.findAll).post(todoController.createOne);

module.exports = router;
