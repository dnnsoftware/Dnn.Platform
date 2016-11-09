import React, {Component, PropTypes} from "react";
import Label from "dnn-label";
import localization from "../../../localization";
import Card from "../Card/Card";
import Gallery from "../Gallery/Gallery";

class LayoutSelector extends Component {

    onCardClick(cardId) {
        console.log("clicked on " + cardId);
    }

    getLayoutCards() {
        return this.props.layouts.map(layout => {
            return <Card 
                cardId={layout.name}
                onClick={this.onCardClick.bind(this)}
                hoverText={localization.get("SetPageLayout")}
                label={layout.name}
                image={layout.thumbnail}
                size="small" />;
        });
    }

    render() {        
        return (
            <div>
                <Label 
                    label={localization.get("Layout")} 
                    tooltipMessage={localization.get("PageLayoutTooltip")} />
                <Gallery>
                    {this.getLayoutCards()}
                </Gallery>
            </div>
        );
    }
}

LayoutSelector.propTypes = {
    currentLayout: PropTypes.object.isRequired,
    layouts: PropTypes.array.isRequired
};

export default LayoutSelector;