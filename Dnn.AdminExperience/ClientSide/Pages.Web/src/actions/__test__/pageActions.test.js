jest.mock("../../services/pageService");
jest.mock("../../utils");
import pageActions from "../pageActions";
import utils from "../../utils";
import PagesService from "../../services/pageService";

describe("Dnn Page Actions", () => {
  it("Redirects when parent page is deleted and the child page is the current page", (done) => {
    // Arrange
    const pageToDelete = {
      tabId: 1,
      hasChild: true,
    };
    const defaultUrl = "http://localhost";
    const redirectUrl = "http://localhost/test";
    PagesService.deletePage.mockResolvedValue({});
    PagesService.getPageHierarchy.mockResolvedValue([1, 2]);
    utils.getCurrentPageId.mockReturnValue(2);
    utils.getDefaultPageUrl.mockReturnValue(redirectUrl);
    delete window.top.location;
    window.top.location = new URL(defaultUrl);
    Object.defineProperty(window.top.location, "href", {
      set: (value) => {
        // Assert
        expect(value).toBe(redirectUrl);
        done();
      },
    });
    expect.assertions(1);

    // Act
    pageActions.deletePage(pageToDelete, null)(() => {});
  });
});
