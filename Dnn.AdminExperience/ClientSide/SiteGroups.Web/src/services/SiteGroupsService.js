import util from "utils";

class SiteGroupsService {
  getServiceFramework() {
    let sf = util.sf;

    sf.moduleRoot = "PersonaBar";
    sf.controller = "SiteGroups";

    return sf;
  }
  getSiteGroups() {
    const sf = this.getServiceFramework();
    return sf.get("GetSiteGroups", {});
  }
  getUnassignedSites() {
    const sf = this.getServiceFramework();
    return sf.get("GetAvailablePortals", {});
  }
  save(siteGroup) {
    const sf = this.getServiceFramework();
    return sf.post("Save", siteGroup);
  }
  delete(siteGroupId) {
    const sf = this.getServiceFramework();
    return sf.post("Delete?groupId=" + siteGroupId, {});
  }
}
const siteGroupsService = new SiteGroupsService();
export default siteGroupsService;
