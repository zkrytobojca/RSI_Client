# Events Białystok Client  - WPF Application

## About
The goal of the project was to create an information service for Białystok. Within it, users can browse the list of available events. They can download events in a time frame covering a day, a week, or preferably download all available events. The application also provides the possibility to download detailed information about one specific event. Each user also has the option of downloading a list of events in PDF format. The app also has the option of logging in and registering users. User accounts are divided into regular users and administrator accounts. Using the administrator account, you can also add, delete and modify available events. 

Two applications were created for the project. The first one - the client application is a window application written using WPF technology and the C# language. The second application - RESTful API - was created in Java using the Spring framework (not included in this repository). Its implementation involved sending binary attachments (@MTOM) while sending files in PDF format and SSL/TLS encryption, which improved the security of transferred data.

## Technologies used
- C#
- WPF
- REST APIs

## Highlights
Event list with CRUD functionality:

![1](https://github.com/zkrytobojca/RSI_Client/assets/49489021/9036b82d-6f12-43b9-8456-2e92e22be0ef)

Generating PDFs with list of events:

![2](https://github.com/zkrytobojca/RSI_Client/assets/49489021/c3cf35e6-331e-4b57-a290-d57d781d86e6)

User Login / Registration:

![3](https://github.com/zkrytobojca/RSI_Client/assets/49489021/6fd54454-522d-409c-85ed-3c9c59ffa79d)

Admin Window view:

![4](https://github.com/zkrytobojca/RSI_Client/assets/49489021/76609434-9374-48ed-84ed-112c14294bd7)
