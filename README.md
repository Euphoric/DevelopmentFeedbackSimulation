# Simple monte-carlo simulation how development cycle time is affected by testing strategy

Each run of simulation consists of these steps:

* Development
* Per-commit automated tests
* Daily automted tests
* Manual tests

Whole process represents a feature being implemented and tested before being declared as done.
Feature starts in development, where time is spent. There is a chance that error is introduced. Then, each consecutive testing stage follows. Each has time of how long it takes and a chance of finding an error, if it exists. If error is found, the process goes back to development, but this time, the fix time is shorter and chance of introducing an error is lower. Probability of each testing stage is "what is chance that error is found if previous stage didn't find any". The process ends when all testing stages pass without finding an error.

Output of each simulation is how long the whole process went and if there is still an error even when going through all the testing stages. Those results are aggregated when thousands of simulations are run.

## Results of some simulations

### Minimal testing
Development process with minimal testing.
Missed errors probability: 37.2%
Development time average: 17.8 h
50 percentile : 13.8 h
70 percentile : 18.9 h
85 percentile : 26.4 h
95 percentile : 42.1 h
### Heavy manual testing
Process where most testing is done manually
Missed errors probability: 14.5%
Development time average: 36.0 h
50 percentile : 31.8 h
70 percentile : 42.2 h
85 percentile : 53.7 h
95 percentile : 71.7 h
### Mostly integration tests
Testing where slow integration tests are used. Allows for lower manual testing.
Missed errors probability: 8.9%
Development time average: 24.6 h
50 percentile : 21.3 h
70 percentile : 27.9 h
85 percentile : 36.5 h
95 percentile : 50.4 h
### Unit + integration tests
Testing with some unit tests and integration tests. Allows for low manual testing.
Missed errors probability: 5.6%
Development time average: 18.9 h
50 percentile : 15.9 h
70 percentile : 20.8 h
85 percentile : 27.7 h
95 percentile : 40.6 h
### Better unit + integration tests
Testing with good unit tests and integration tests. Allows for minimal manual testing.
Missed errors probability: 1.8%
Development time average: 15.0 h
50 percentile : 12.2 h
70 percentile : 16.1 h
85 percentile : 21.7 h
95 percentile : 32.5 h
### Continuous delivery
Testing with highly reliable unit tests and integration tests and almost no manual testing.
Missed errors probability: 0.5%
Development time average: 13.8 h
50 percentile : 10.9 h
70 percentile : 14.6 h
85 percentile : 20.3 h
95 percentile : 31.5 h
### Heavy manual testing + shorter dev time
Same as Heavy manual testing but with shorter development times
Missed errors probability: 14.5%
Development time average: 29.9 h
50 percentile : 25.7 h
70 percentile : 35.2 h
85 percentile : 45.8 h
95 percentile : 61.3 h
### Continuous delivery + shorter dev time
Same as Continuous delivery but with shorter development times
Missed errors probability: 0.7%
Development time average: 7.5 h
50 percentile : 6.9 h
70 percentile : 8.2 h
85 percentile : 10.0 h
95 percentile : 12.8 h