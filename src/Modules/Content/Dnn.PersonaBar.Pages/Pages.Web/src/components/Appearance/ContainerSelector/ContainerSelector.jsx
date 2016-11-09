import React, {Component, PropTypes} from "react";
import Label from "dnn-label";
import localization from "../../../localization";
import Card from "../Card/Card";
import Gallery from "../Gallery/Gallery";

class ContainerSelector extends Component {

    onCardClick(cardId) {
        console.log("clicked on " + cardId);
    }

    getContainerCards() {
        return this.props.containers.map(container => {
            return <Card 
                cardId={container.name}
                onClick={this.onCardClick.bind(this)}
                hoverText={localization.get("SetPageContainer")}
                label={container.name}
                image={container.thumbnail}
                size="small" />;
        });
    }

    render() {        
        return (
            <div>
                <Label 
                    label={localization.get("PageContainer")} 
                    tooltipMessage={localization.get("PageContainerTooltip")} />
                <Gallery>
                    {this.getContainerCards()}
                </Gallery>
            </div>
        );
    }
}

ContainerSelector.propTypes = {
    currentContainer: PropTypes.object.isRequired,
    containers: PropTypes.array.isRequired
};

export default ContainerSelector;