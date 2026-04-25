const mongoose = require("mongoose");

const schema = new mongoose.Schema({
  name: { type: String, unique: true },
  seq: { type: Number, default: 0 },
});

const Counter = mongoose.model("Counter", schema);

async function getNextSequence(name) {
  const counter = await Counter.findOneAndUpdate(
    { name },
    { $inc: { seq: 1 } },
    { returnDocument: "after", upsert: true },
  );

  return counter.seq;
}

async function resetSequence(name) {
  const counter = await Counter.findOneAndDelete(
    { name: name },
    { $set: { seq: 0 } },
    { new: true, upsert: true },
  );
}

async function deleteSequence(name) {
  await Counter.deleteOne({ name: name });
}

module.exports = {
  getNextSequence,
  resetSequence,
  deleteSequence,
};
