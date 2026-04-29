import axios from "axios";

//axios.defaults.baseURL = "http://172.28.48.1:5000/server/api";
axios.defaults.baseURL = "http://localhost:5000/server/api";

export const API = {
  get: async (url) => {
    return axios
      .get(url)
      .then((response) => {
        return response;
      })
      .catch((error) => console.error(error.message));
  },
  delete: async (url) => {
    return axios
      .delete(url)
      .then((response) => {
        return response;
      })
      .catch((error) => console.error(error.message));
  },
  post: async (url, data) => {
    return axios
      .post(url, data)
      .then((response) => {
        return response;
      })
      .catch((error) => console.error(error.message));
  },
  patch: async (url, data) => {
    return axios
      .patch(url, data)
      .then((response) => {
        return response;
      })
      .catch((error) => console.error(error.message));
  },
};
