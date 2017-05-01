module.exports = {
            "Name": "Home",
            "TabId": "20",
            "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
            "Tooltip": "Homepage of the site",
            "ParentTabId": 0,
            "HasChildren": true,
            "IsOpen": false,
            "Selectable": true,
            "CheckedState": 0,
            "ChildTabs": [
                {
                "Name": "Inner Child",
                "TabId": "211",
                "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
                "Tooltip": "Page is visible to everyone",
                "ParentTabId": 20,
                "HasChildren": true,
                "IsOpen": false,
                "Selectable": true,
                "CheckedState": 0,
                "ChildTabs": [
                     {
                        "Name": "Inner Inner Child",
                        "TabId": "21111",
                        "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
                        "Tooltip": "Page is visible to everyone",
                        "ParentTabId": "211",
                        "HasChildren": true,
                        "IsOpen": false,
                        "Selectable": true,
                        "CheckedState": 0,
                        "ChildTabs": [
                            {
                            "Name": "Inner Inner Inner Child",
                            "TabId": "001",
                            "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
                            "Tooltip": "Page is visible to everyone",
                            "ParentTabId": "21111",
                            "HasChildren": false,
                            "IsOpen": false,
                            "Selectable": true,
                            "CheckedState": 0,
                            "ChildTabs":[]
                          }
                      ]
                    },
                    {
                       "Name": "Inner Inner Child 2",
                       "TabId": "222",
                       "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
                       "Tooltip": "Page is visible to everyone",
                       "ParentTabId": 211,
                       "HasChildren": false,
                       "IsOpen": false,
                       "Selectable": true,
                       "CheckedState": 0,
                       "ChildTabs": []
                   },
                ]
              },
              {
                  "Name": "Our Products",
                  "TabId": "213",
                  "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
                  "Tooltip": "Page is visible to everyone",
                  "ParentTabId": 20,
                  "HasChildren": false,
                  "IsOpen": false,
                  "Selectable": true,
                  "CheckedState": 0,
                  "ChildTabs": []
              }

            ]
        }
