import React, {Component, PropTypes} from "react";
import { Scrollbars } from "react-custom-scrollbars";
import style from "./style.less";

class Gallery extends Component {

    calculateGalleryWidth() {
        const { children, size } = this.props;
        const elementSize = size === "big" ? 198 : 130;
        return elementSize * children.length;
    }

    render() {  
        const width = this.calculateGalleryWidth();    
        return (
            <div className={style.moduleContainer}>
                <Scrollbars
                    className="container"
                    autoHeight
                    autoHeightMin={0}
                    autoHeightMax={480}>
                    <div style={{width}}>
                        {this.props.children}
                    </div>
                </Scrollbars>
            </div>
        );
    }
}

Gallery.propTypes = {
    children: PropTypes.node,
    size: PropTypes.oneOf(["small", "big"])
};

Gallery.defaultProps = {
    size: "big"
};

export default Gallery;