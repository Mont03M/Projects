const handleFacotry = require("../handleFactory/handlerFactory");
const Todo = require("../models/todoModel");

exports.getOne = handleFacotry.getOne(Todo);
exports.getList = handleFacotry.getList(Todo);
exports.createOne = handleFacotry.createOne(Todo);
exports.updateOne = handleFacotry.updateOne(Todo);
exports.updateMany = handleFacotry.updateMany(Todo);
exports.deleteOneById = handleFacotry.deleteOneById(Todo);
exports.deleteOne = handleFacotry.deleteOne(Todo);
exports.findAll = handleFacotry.findAll(Todo);
exports.searchAll = handleFacotry.searchAll(Todo);
