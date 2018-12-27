import React, {Component} from "react";
import PropTypes from "prop-types";
import { Label } from "@dnnsoftware/dnn-react-common";
import localization from "../../../localization";
import Card from "../Card/Card";
import Gallery from "../Gallery/Gallery";

class ContainerSelector extends Component {

    onContainerClick(containerName) {
        const container = this.props.containers.find(c => c.name === containerName);
        this.props.onSelectContainer(container);
    }

    isSelected(container) {
        const { selectedContainer } = this.props;
        if (!selectedContainer) {
            return false;
        }
        return selectedContainer.name === container.name;
    }

    getSelectedIndex() {
        const { selectedContainer, containers } = this.props;
        if (!selectedContainer) {
            return -1;
        }
        return containers.findIndex(c => c.name === selectedContainer.name);
    }

    getContainerCards() {
        const { containers, noThemeSelected } = this.props;
        let {defaultPortalContainer} = this.props;
        defaultPortalContainer = defaultPortalContainer === null ? "" : defaultPortalContainer;

        if (noThemeSelected) {
            return <div className="no-appearance-items">{localization.get("NoThemeSelectedForContainers") }</div>;
        }
        if (containers.length === 0) {
            return <div className="no-appearance-items">{localization.get("NoContainers") }</div>;
        }
        return containers.map(container => {
            return <Card
                key={container.name}
                cardId={container.name}
                onClick={this.onContainerClick.bind(this) }
                hoverText={localization.get("SetPageContainer") }
                label={container.name}
                isSiteDefault={defaultPortalContainer.toString().toLowerCase() === container.path.toString().toLowerCase() }
                image={container.thumbnail}
                selected={this.isSelected(container) }
                size="small" />;
        });
    }

    render() {
        const selectedIndex = this.getSelectedIndex();
        return (
            <div>
                <Label
                    label={localization.get("PageContainer") }
                    tooltipMessage={localization.get("PageContainerTooltip") } />
                <Gallery size="small" scrollToIndex={selectedIndex}>
                    {this.getContainerCards() }
                </Gallery>
            </div>
        );
    }
}

ContainerSelector.propTypes = {
    noThemeSelected: PropTypes.bool.isRequired,
    selectedContainer: PropTypes.object,
    containers: PropTypes.array.isRequired,
    onSelectContainer: PropTypes.func.isRequired,
    defaultPortalContainer: PropTypes.string
};

export default ContainerSelector;