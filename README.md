# Tenure Listener

One of the Manage My Home _(MMH)_ solution event-driven applications that updates the Tenure DynamoDB
table documents _(records)_ by listenening and processing the following events:

1. PersonCreatedEvent _(only V1) - V1 appears to mean 'PersonAddedToTenure'_
2. PersonUpdatedEvent
3. AccountCreatedEvent

Due to how the MMH data is structured and stored, some of the Person DynamoDB table information is duplicated
into the Tenure DynamoDB document under HouseholdMembers key in a form of Person objects rather than being
referenced through GUIDs.

* As a consequence of the architecture, a Tenure Listener is needed to append new people to HouseholdMembers
whenever they're added. _(PersonCreatedEvent V1)_

* As a consequence of Tenure document duplicating Person data rather than referencing it via Id, a Tenure Listener
is needed to perform the behind-the-curtain Person data plumbing to keep the Tenure and Person DynamoDB tables in
sync _(PersonUpdatedEvent)_.

* Due to a similar issue as with Person data duplication, a Tenure listener is needed to sync the Payment Reference
Number (PRN) duplicated within Tenure DynamoDB document with the number that gets generated for the _(finance)_
Account DynamoDB document _(AccountCreatedEvent)_.
