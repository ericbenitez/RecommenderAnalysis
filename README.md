
Lab 1 - Reflection questions
---------------------------------------------------------------
a. What resources do you have and what is their structure?
This application makes use of the Blazor WASM framework, which consists of:
- A client with Razor Components (code is written in C# and compiled into WASM format)
- A server with Services & Controllers
    - Services are singletons which run in the background to execute tasks
    - Controllers are where our endpoints are located

---------------------------------------------------------------
b. Which HTTP methods are you using for various operations and why?
- [GET /api/products/] in order to get all products
- [GET /api/products/{id}] in order to get a product by Id
- [GET /api/products/{id}/reviews] in order to get the reviews of a product

- [POST /api/products/create] in order to create a product with a Json string
- [POST /api/products/{id}/reviews/create] in order to submit a review

---------------------------------------------------------------
c. What is your URI naming scheme?
- Everything is in lower case and for POST requests the final keyword represents the action

---------------------------------------------------------------
d. What response codes are you sending and why?
- I send a Status code 200 if it was successfull and 400 if the input was invalid (otherwise Blazor responds with something else)
