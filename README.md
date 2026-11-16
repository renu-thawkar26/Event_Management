**NSQ Event Management System**

Project Overview
The NSQ Event Management System is a Microsoft Dynamics 365 CRM and Power Platform solution designed to manage the complete lifecycle of events, sessions, speakers, attendees, registrations, and feedback.
The application enables organizations to plan events, schedule sessions, manage speakers and attendees, automate registrations, collect feedback, and generate insights through dashboards and reporting.
This project was developed using:
•	Microsoft Dataverse
•	Model-Driven Apps
•	JavaScript
•	C# Plugins
•	Power Automate
•	Business Rules
•	Business Process Flows (BPF)
•	Solution Management & ALM
•	GitHub Source Control
The solution follows real-world Dynamics 365 development practices with validations, automation, security, reporting, deployment, and documentation. 
________________________________________
**Tables Implemented**

**Event**

Manages event details such as Event Name, Dates, Location, Event Type, Status, and Event Code. Supports Public and Corporate Event Business Process Flows. 

**Session**

Stores session information including Session Title, Event, Speaker, Schedule, Session Type, and Status. 

**Speaker**

Maintains speaker information such as Name, Email, Phone Number, Organization, Biography, and LinkedIn Profile. 

**Attendee**

Stores attendee details including Name, Email, Phone, Company, Job Title, and Preferred Contact Method. 

**Event Registration**

Handles attendee registrations for events, confirmation numbers, event dates, and registration status management. 

**Feedback**

Captures attendee feedback for completed sessions, including Rating, Comments, Submission Date, and Feedback Number. 

________________________________________
**Relationship Diagram**

Event (1) -------- (N) Session

Speaker (1) ------ (N) Session

Event (1) -------- (N) Event Registration

Attendee (1) ----- (N) Event Registration

Session (1) ------ (N) Feedback

Attendee (1) ----- (N) Feedback
________________________________________
**Project Flow**

Create Event


	↓

Assign Business Process Flow


	↓


Create Session


	↓


Assign Speaker


	↓


Schedule Event


	↓


Register Attendees


	↓
	

Mark Attendance


	↓


Collect Feedback


	↓


Close Event
________________________________________
**Team Contributions**

Nilay


Event Registration

•	Event registration validations

•	Registration confirmation generation

•	Duplicate registration prevention

•	Registration status management

•	Registration-related automation and notifications



Feedback

•	Feedback validations

•	Rating and comments handling

•	Feedback submission restrictions

•	Session completion validations

•	Feedback-related automation

________________________________________
Renu


Attendee

•	Attendee table configuration

•	Form customization

•	Business validations

•	JavaScript, Plugin and Business Rule implementation



Speaker
•	Speaker table configuration

•	Form customization

•	Validation implementation

•	JavaScript, Plugin and Business Rule implementation

________________________________________
Anup


Event

•	Event management functionality

•	Business Process Flows

•	Event validations

•	Event automation


Session
•	Session scheduling

•	Session validations

•	Speaker assignment management

•	Session automation
________________________________________
**Features Implemented**


JavaScript

•	Form validations

•	Dynamic field behavior

•	Mandatory field handling

•	Read-only field management

•	Client-side business logic



Plugins

•	Server-side validations

•	Data integrity enforcement

•	Business rule implementation

•	Restriction and verification logic



Power Automate
•	Email notifications

•	Automated status updates

•	Registration and session-based automation



Business Rules

•	Field visibility control

•	Field requirements

•	Form-level business conditions



Business Process Flows

•	Public Event Process

•	Corporate Event Process


________________________________________

**Solution Components**

•	Dataverse Tables

•	Model-Driven Application

•	Forms

•	Views

•	JavaScript Web Resources

•	Plugin Assembly

•	Plugin Steps

•	Power Automate Flows

•	Business Rules

•	Business Process Flows

•	Managed Solution Deployment

________________________________________
*Technology Stack*

Technology	Purpose: Microsoft Dynamics 365	CRM Platform

Microsoft Dataverse: 	Data Storage

Power Apps:	Model-Driven App

JavaScript:	Client-Side Validations

C# Plugins:	Server-Side Validations

Power Automate:	Workflow Automation

GitHub:	Source Control

________________________________________
**Repository Structure**
________________________________________
Developed As Part Of

**NSQUARE XPERTS - Event Management System Project**

A hands-on Microsoft Dynamics 365 CRM and Power Platform solution implementing industry-standard practices including Dataverse customization, JavaScript development, Plugin development, Power Automate automation, Business Process Flows, ALM, Deployment, and GitHub Source Control. 
________________________________________
**Project Team**

•	Nilay - Event Registration & Feedback

•	Renu - Attendee & Speaker

•	Anup - Event & Session

_Built using Microsoft Dynamics 365 CRM, Dataverse, Power Platform, Plugins, JavaScript, and Power Automate._

