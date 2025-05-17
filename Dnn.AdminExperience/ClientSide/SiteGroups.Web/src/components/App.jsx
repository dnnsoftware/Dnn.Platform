import React from "react";
import NewSiteGroup from "./NewGroup";
import SiteGroupsTable from "./Table";
import { GridCell, PersonaBarPageHeader } from "@dnnsoftware/dnn-react-common";
import service from "../services/SiteGroupsService";
import utils from "../utils";
import Resx from "../localization";

export default class SiteGroupApp extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            groups: [],
            currentGroup: null,
            unassignedSites: []
        };
    }

    componentDidMount() {
        this.loadState();
    }

    loadState() {
        const self = this;
        service.getSiteGroups().then(groups => self.setState({ groups: groups }));
        service
            .getUnassignedSites()
            .then(sites => self.setState({ unassignedSites: sites }));
    }

    editNewGroup(id) {
        const site = this.state.unassignedSites.find(s => s.PortalId === id);
        this.setState({
            currentGroup: {
                PortalGroupId: -1,
                MasterPortal: site,
                PortalGroupName: site.PortalName + " " + Resx.get("Group"),
                AuthenticationDomain: "",
                Portals: []
            }
        });
    }

    save(r) {
        const self = this;
        const group = r.PortalGroup;
        const unassignedSites = r.UnassignedSites;
        const isNewGroup = group.PortalGroupId === -1;
        service.save(group).then(id => {
            if (isNewGroup) group.PortalGroupId = id;
            const groups = (isNewGroup
                ? self.state.groups
                : self.state.groups.filter(g => g.PortalGroupId !== group.PortalGroupId)
            )
                .concat([group])
                .sort((a, b) => (a.PortalGroupName < b.PortalGroupName ? -1 : 1));
            self.setState({
                unassignedSites: unassignedSites,
                currentGroup: null,
                groups: groups
            });
        });
    }

    deleteGroup(group) {
        const self = this;
        utils.confirm(
            Resx.get("DeleteGroup.Confirm"),
            Resx.get("Delete"),
            Resx.get("Cancel"),
            () => {
                service.delete(group.PortalGroupId).then(() => {
                    self.setState({
                        unassignedSites: self.state.unassignedSites
                            .concat(group.Portals)
                            .concat([group.MasterPortal])
                            .sort((a, b) => (a.PortalName < b.PortalName ? -1 : 1)),
                        groups: self.state.groups.filter(
                            g => g.PortalGroupId !== group.PortalGroupId
                        ),
                        currentGroup: null
                    });
                });
            }
        );
    }

    render() {
        return (
            <GridCell>
                <PersonaBarPageHeader title={Resx.get("nav_SiteGroups")}>
                    <NewSiteGroup
                        unassignedSites={
                            this.state.currentGroup ? [] : this.state.unassignedSites
                        }
                        disabled={this.state.currentGroup !== null}
                        onNewGroup={siteId => this.editNewGroup(Number(siteId))}
                    />
                </PersonaBarPageHeader>
                <SiteGroupsTable
                    groups={this.state.groups}
                    unassignedSites={this.state.unassignedSites}
                    currentGroup={this.state.currentGroup}
                    onEditGroup={group => this.setState({ currentGroup: group })}
                    onCancelEditing={() => this.setState({ currentGroup: null })}
                    onDeleteGroup={group => this.deleteGroup(group)}
                    onSave={result => this.save(result)}
                />
            </GridCell>
        );
    }
}
