// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

/// @notice Minimal on-chain hash registry for SovereignID Phase 1 demos.
contract Notary {
    mapping(bytes32 => uint256) public timestamps;

    function notarize(bytes32 hash) external {
        timestamps[hash] = block.timestamp;
    }
}
