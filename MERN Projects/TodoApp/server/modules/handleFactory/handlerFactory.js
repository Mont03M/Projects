const catchAsync = require("../utils/catchAsync");

exports.createOne = (Model) =>
  catchAsync(async (req, res, next) => {
    const doc = await Model.create(req.body);

    return res.status(200).json({
      success: true,
      data: doc,
    });
  });

exports.getOne = (Model) =>
  catchAsync(async (req, res) => {
    const doc = await Model.findById(req.params.id);

    if (!doc) {
      return res.status(404).json({
        success: false,
        message: "Document not found",
      });
    }

    return res.status(200).json({
      success: true,
      data: doc,
    });
  });

exports.getList = (Model) =>
  catchAsync(async (req, res) => {
    const query = { ...req.query };
    if (query.dueDate) {
      const start = new Date(`${query.dueDate}T00:00:00.000Z`);
      const end = new Date(`${query.dueDate}T23:59:59.999Z`);

      query.dueDate = {
        $gte: start,
        $lte: end,
      };
    }

    const docs = await Model.find(query);

    if (!docs) {
      return res.status(404).json({
        success: false,
        message: "Documents not found!",
      });
    }

    return res.status(200).json({
      success: true,
      count: docs.length,
      data: docs,
    });
  });

exports.findAll = (Model) =>
  catchAsync(async (req, res) => {
    const docs = await Model.find({});

    if (!docs) {
      return res.status(404).json({
        success: false,
        message: "Documents not found!",
      });
    }

    return res.status(200).json({
      success: true,
      count: docs.length,
      data: docs,
    });
  });

exports.updateOne = (Model) =>
  catchAsync(async (req, res) => {
    const doc = await Model.findByIdAndUpdate(req.params.id, req.body, {
      returnDocument: "after",
      runValidators: true,
    });

    if (!doc) {
      return res.status(404).json({
        success: false,
        message: "Document not found!",
      });
    }

    return res.status(200).json({
      success: true,
      data: doc,
    });
  });

exports.updateMany = (Model) =>
  catchAsync(async (req, res) => {
    const updates = req.body;

    const operations = updates.map((item) => ({
      updateOne: {
        filter: { _id: item._id },
        update: { $set: item },
      },
    }));

    const result = await Model.bulkWrite(operations);

    return res.status(200).json({
      success: true,
      data: result,
    });
  });

exports.deleteOneById = (Model) =>
  catchAsync(async (req, res) => {
    const doc = await Model.findByIdAndDelete(req.params.id);

    if (doc) {
      return res.status(200).json({
        success: true,
        data: doc,
      });
    }

    return res.status(404).json({
      success: false,
      message: "Document not deleted!",
    });
  });

exports.deleteOne = (Model) =>
  catchAsync(async (req, res) => {
    const doc = await Model.findOneAndDelete(req.body);

    if (doc) {
      return res.status(200).json({
        success: true,
        data: doc,
      });
    }

    return res.status(404).json({
      success: false,
      message: "Document not deleted!",
    });
  });

exports.searchAll = (Model) =>
  catchAsync(async (req, res) => {
    let { searchParams = [] } = req.body;

    if (!Array.isArray(searchParams)) {
      return res.status(400).json({
        success: false,
        message: "searchParams must be an array",
      });
    }

    const docs = await Model.find({});

    const filtered = docs.filter((doc) => {
      const values = Object.values(doc.toObject())
        .filter((v) => v !== null && v !== undefined)
        .map((v) => {
          if (v instanceof Date) {
            return v.toISOString().split("T")[0];
          }
          return String(v).toLowerCase().trim();
        });

      return searchParams.every((param) =>
        values.includes(String(param).toLowerCase().trim()),
      );
    });

    return res.status(200).json({
      success: true,
      count: filtered.length,
      data: filtered,
    });
  });
