import siteInfo from "../siteInfoReducer";
import { siteInfo as ActionTypes } from "../../constants/actionTypes";

describe("Site Info Reducer", () => {
  it("Sets the current portal id correctly when there are multiple sites", () => {
    // Arrange
    let action = {
      type: ActionTypes.RETRIEVED_PORTALS,
      data: {
        portals: [
          {
            PortalID: 0,
            IsCurrentPortal: false,
          },
          {
            PortalID: 1,
            IsCurrentPortal: true,
          },
        ],
      },
    };

    // Act
    let state = siteInfo({}, action);

    // Assert
    expect(state.portalId).toEqual(1);
  });
});
