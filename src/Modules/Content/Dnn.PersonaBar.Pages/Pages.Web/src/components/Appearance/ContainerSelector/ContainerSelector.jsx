import React, {Component, PropTypes} from "react";
import Label from "dnn-label";
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

    getContainerCards() {
        return this.props.containers.map(container => {
            return <Card 
                cardId={container.name}
                onClick={this.onContainerClick.bind(this)}
                hoverText={localization.get("SetPageContainer")}
                label={container.name}
                image={container.thumbnail}
                selected={this.isSelected(container)}
                size="small" />;
        });
    }

    render() {        
        return (
            <div>
                <Label 
                    label={localization.get("PageContainer")} 
                    tooltipMessage={localization.get("PageContainerTooltip")} />
                <Gallery size="small">
                    {this.getContainerCards()}
                </Gallery>
            </div>
        );
    }
}

ContainerSelector.propTypes = {
    selectedContainer: PropTypes.object,
    containers: PropTypes.array.isRequired,
    onSelectContainer: PropTypes.func.isRequired
};

export default ContainerSelector;