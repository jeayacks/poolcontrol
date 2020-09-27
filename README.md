# Raspberry pool control

The Rapsberry solution which automates your swimming pool !

Manages pump cycles depending on water temperature and protect your swimming pool during the winter against frost.

Web APIs enable integration with your favorite home automation system.

![.NET Core](https://github.com/bluewaterwise/poolcontrol/workflows/.NET%20Core/badge.svg)

Example of integration in [Home assistant](https://www.home-assistant.io/):

![Home assistant integration](doc/medias/home-assistant-screenshot.png)

## The context

The initial goal was to automaticaly adjust the pumping duration according the water temperature without modifying the existing system.

![Physical system](doc/medias/swimming-pool-installation.svg)

## The hardware

## The control logic

## Install and configure on raspberry

## 


Only 2 scenarios are possible : Winter and Summer and total pumping time is calculated using the temperature.

The proposal is to introduce a intermediate group of pump cycles depending on temperature range

Current solution
```json
{
  "SummerPumpingCycles": [
    {
      "DecisionTime": "02:00:00"
    },
    {
      "DecisionTime": "09:00:00"
    }
  ]
}

```


Proposal :
```json
{
  "SummerPumpingCycles":[
      {
        "name": "default cycles",
        "range": "default",
        "cycles":[
            {
            "DecisionTime": "02:00:00"
            },
            {
            "DecisionTime": "09:00:00"
            }
        ]
      },
      {
        "name": "Lower than 20°",
        "range": "0-20",
        "cycles":[
            {
            "DecisionTime": "02:00:00"
            },
            {
            "DecisionTime": "09:00:00"
            }
        ]
      }
  ]
  }
}
```
