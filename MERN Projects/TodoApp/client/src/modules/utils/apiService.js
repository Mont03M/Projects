import axios from "axios";

//axios.defaults.baseURL = "http://172.28.48.1:5000/server/api";
axios.defaults.baseURL = "http://localhost:5000/server/api";

export const API = {
  get: (url) => axios.get(url),
  put: (url) => axios.put(url),
  post: (url, data) => axios.post(url, data),
  patch: (url, data) => axios.patch(url, data),
};
