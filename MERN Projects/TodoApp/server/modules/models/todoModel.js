const mongoose = require("mongoose");
const {
  getNextSequence,
  resetSequence,
  deleteSequence,
} = require("../utils/counter");

const schema = new mongoose.Schema({
  name: {
    type: String,
  },
  task: {
    type: String,
    required: [true, "must enter a task"],
  },
  description: {
    type: String,
  },
  status: {
    type: String,
    enum: ["Completed", "Pending"],
    required: [true, "must enter a status"],
  },
  priority: {
    type: String,
    enum: ["High", "Medium", "Low"],
    required: [true, "Must enter a priority level"],
  },
  dueDate: {
    type: Date,
    required: [true, "Must enter a due date"],
    default: Date.now,
  },
  taskId: {
    type: Number,
  },
});

schema.pre("save", async function () {
  if (!this.taskId) {
    const seq = await getNextSequence("todo");

    this.taskId = seq;
    this.name = "TS" + String(seq).padStart(4, "0");
  }
});

const Todo = mongoose.model("Todo", schema);
module.exports = Todo;
