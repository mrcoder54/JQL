# JQL
This is a data mapping library.  Source of data can be a class, XML or JSON.  The data found in the mapping file will instruct the library to fill in data for the return T.

JQL Features - JSON Query Language
---------------------------------------

Given a class/XML/JSON, a return type and a mapping file, library knows how to populate return type
	- mapping file must contain return type property and query
		- i.e. { "name" : "person.name", "address" : "home[0]" }
		- name and address is a property of return type.  person.name and home[0] are queries allowing the library to populate the data
	- return type must be a class

Source of data can be a class, XML or JSON.  The data found in the map file will instruct the library to fill in data for the return T.
Library does not need a reference to the return Type; reflection is used to populate data.

