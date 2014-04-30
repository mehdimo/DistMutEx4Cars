Lamport's Distributed Mutual Exclusion
=====================================================

Problem
-------
There is traffic route around a narrow bridge on a river. Four cars move along the designated routes. 
The bridge is so narrow that at any time, multiple cars cannot pass in opposite directions.

Approach
--------
This program implements a decentralized protocol in which at most one car can cross the bridge at any time, and no car is indefinitely prevented from crossing the bridge.
The communication mechanism is based on message passing in which each car can send/receive message to/from other cars.

We have used Lamport's Distributed Mutual Exclusion algorithm to handle accessing the critical section (bridge).

Usage
-----
To run the program you need to compile the solution in VS 2012 or compatible versions.



