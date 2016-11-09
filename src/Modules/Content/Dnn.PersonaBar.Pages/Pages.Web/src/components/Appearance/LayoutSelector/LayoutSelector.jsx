import React, {Component, PropTypes} from "react";
import Label from "dnn-label";
import localization from "../../../localization";
import Card from "../Card/Card";
import Gallery from "../Gallery/Gallery";

class LayoutSelector extends Component {

    onClickLayout(layoutName) {
        const layout = this.props.layouts.find(l => l.name === layoutName);
        this.props.onSelectLayout(layout);
    }

    isSelected(layout) {
        const { selectedLayout } = this.props;
        if (!selectedLayout) {
            return false;
        }
        return selectedLayout.name === layout.name;
    }

    getLayoutCards() {
        return this.props.layouts.map(layout => {
            return <Card 
                cardId={layout.name}
                onClick={this.onClickLayout.bind(this)}
                hoverText={localization.get("SetPageLayout")}
                label={layout.name}
                image={layout.thumbnail}
                selected={this.isSelected(layout)}
                size="small" />;
        });
    }

    render() {        
        return (
            <div>
                <Label 
                    label={localization.get("Layout")} 
                    tooltipMessage={localization.get("PageLayoutTooltip")} />
                <Gallery size="small">
                    {this.getLayoutCards()}
                </Gallery>
            </div>
        );
    }
}

LayoutSelector.propTypes = {
    selectedLayout: PropTypes.object,
    layouts: PropTypes.array.isRequired,
    onSelectLayout: PropTypes.func.isRequired
};

export default LayoutSelector;