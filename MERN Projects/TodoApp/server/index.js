const express = require("express");
const cors = require("cors");
const loadModuleRoutes = require("./modules/routeLoader/baseRoutes");
const todoRoutes = require("./modules/routes/todoRoutes");
const getLocalIp = require("./modules/utils/localIP");
const dotenv = require("dotenv");
const mongoose = require("mongoose");

const app = express();

dotenv.config({ path: "./.config", override: false });
app.use(cors());
app.use(express.json());

mongoose
  .connect(process.env.MONGO_URI)
  .then((conn) => {
    console.log(`Connected to DB from: ${conn.connection.host}`);
  })
  .catch((error) => {
    console.error("DB connection error: ", error.message);
    process.exit(1);
  });

app.get("/", (req, res) => {
  res.send("API is running");
});

// multiple routes (app routes) run loadModuleRoutes function
// todoRoutes --> path: /server/api/apps (e.g, http://172.28.48.1:5000/server/api/list)
// loadModuleRoutes --> path: /server/api/apps/{App name} (e.g., http://172.28.48.1:5000/server/api/TodoApp/list)
app.use("/server/api/", todoRoutes); // loadModuleRoutes

// process.env.HOST
app.listen(process.env.PORT, () => {
  //console.log(`Server running on http://${getLocalIp()}:${process.env.PORT}`);
  console.log(`Server running on http://localhost:${process.env.PORT}`);
  console.log(`Server accessible on Host ${process.env.HOST}`);
});
