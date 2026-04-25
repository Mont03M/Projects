const path = require("path");
const fs = require("fs");
const modules = require("./moduleRoutes");
const catchAsync = require("../utils/catchAsync");

const loadModuleRoutes = catchAsync(async (req, res, next) => {
  const url = new URL(req.url, `http://${req.headers.host}`);
  const pathname = url.pathname;
  for (const mod of modules) {
    if (mod.pattern.test(pathname)) {
      const router = require(mod.path);

      const strippedPath = pathname.replace(`/${mod.name}`, "") || "/";

      req.url = strippedPath + url.search;

      return router(req, res, next);
    }
  }
});

module.exports = loadModuleRoutes;
