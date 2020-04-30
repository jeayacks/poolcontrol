# poolcontrol
Swimming pool control automation

Automate you swimming pool using RPI 

Manages pump cycles depending on water and air temperatures and manage chimical products injections.

Web APIs enable Home automation integration.

![.NET Core](https://github.com/bluewaterwise/poolcontrol/workflows/.NET%20Core/badge.svg)




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
