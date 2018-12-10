import React, {Component} from "react";
import PropTypes from "prop-types";
import { TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";
import styles from "./style.less";
import localization from "../../../localization";

class Card extends Component {

    onClick() {
        this.props.onClick(this.props.cardId);
    }

    getNoImageIcon() {
        return <svg version="1.1" x="0px" y="0px" viewBox="0 0 2048 2048">
            <g>
                <rect x="255.2" y="254.3" width="480.1" height="523.2"/>
                <rect x="875.8" y="589.7" width="917" height="187.8"/>
                <rect x="255.2" y="905.9" width="1537.6" height="883.1"/>
                <rect x="875.8" y="254.3" width="917" height="187.8"/>
            </g>
        </svg>;
    }

    getCheckMarkIcon() {
        return <svg version="1.1" x="0px" y="0px" viewBox="0 0 2048 2048">
            <g>
                <polygon points="1524.4,714.3 1417,606.9 868,1155.8 657.8,945.6 550.4,1053 868,1370.7 975.5,1263.3 975.5,1263.3"/>
            </g>
        </svg>;
    }

    getImageComponent() {
        const {image, selected, hoverText, label, isSiteDefault  } = this.props;
        const className = "card-image" + (image ? "" : " no-image") + (isSiteDefault ? " site-default" : "");

        return <span className={className} onClick={this.onClick.bind(this) } >
            {image ? <img src={image} alt={label} /> : this.getNoImageIcon() }
            {selected && <span className={(isSiteDefault ? 'checkmark site-default' : 'checkmark') }>{this.getCheckMarkIcon() }</span>}
            <span className="hoverLayer">{hoverText}</span>
        </span>;
    }

    render() {
        const { selected, size, isSiteDefault } = this.props;
        const className = styles.moduleContainer + (selected ? " selected" : "") + " " + size;
        const maxWidth = size === "big" ? 168 : 100;
        return (
            <div className={className}>
                {this.getImageComponent() }
                <TextOverflowWrapper text={this.props.label} maxWidth={maxWidth} className={(isSiteDefault ? 'card-title site-default' : 'card-title') }  />
                <TextOverflowWrapper text={isSiteDefault && localization.get("SiteDefault") } maxWidth={maxWidth} className="card-title-site-default" />
            </div>
        );
    }
}

Card.propTypes = {
    cardId: PropTypes.string.isRequired,
    onClick: PropTypes.func.isRequired,
    hoverText: PropTypes.string.isRequired,
    label: PropTypes.string.isRequired,
    image: PropTypes.string,
    selected: PropTypes.bool,
    size: PropTypes.oneOf(["big", "small"]),
    isSiteDefault: PropTypes.bool
};

Card.defaultProps = {
    size: "big",
    selected: false,
    isSiteDefault: false
};

export default Card;